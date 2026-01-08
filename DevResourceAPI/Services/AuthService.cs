using System.Security.Cryptography;
using System.Text;
using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace DevResourceAPI.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
{
    _context = context;
    _configuration = configuration; // İşte eksik olan parça buydu!
}

    // Kullanıcı Kaydı
    public async Task<User?> Register(string username, string password)
    {
        // Kullanıcıya net bir uyarı:
    if (password.Length < 6)
    {
        // Bu hata Swagger'da kullanıcıya görünür
        throw new Exception("Şifre en az 6 karakter olmalıdır. Test için: 123456 kullanabilirsin.");
    }

    if (await UserExists(username)) return null;
        // 1. Kullanıcı zaten var mı kontrolü
        if (await UserExists(username)) return null;

        // 2. Hash ve Salt oluşturma
        CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // Kullanıcı Girişi
    public async Task<User?> Login(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        
        // Kullanıcı yoksa veya şifre yanlışsa null dön
        if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            return null;

        return user;
    }

    // Yardımcı Metod: Kullanıcı varlık kontrolü
    public async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    // Yardımcı Metod: Şifreleme (Out parametreli kullanım profesyoneldir)
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA256();
        passwordSalt = hmac.Key; // Üretilen rastgele anahtar
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Şifrenin karma hali
    }

    // Yardımcı Metod: Doğrulama
    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA256(storedSalt); // Veritabanındaki salt'ı kullan
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(storedHash); // SequenceEqual çok daha performanslıdır
    }
    // Kullanıcı Silme
    public async Task<bool> DeleteUser(string username, string password)
{
    var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
    
    // Kullanıcı yoksa veya şifre yanlışsa silme işlemini reddet
    if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        return false;

    _context.Users.Remove(user);
    await _context.SaveChangesAsync();
    return true;
}



public string CreateToken(User user)
{
    // Kullanıcı bilgilerini (Claim) token içine yerleştiriyoruz
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };

    // Anahtarı appsettings'ten oku
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
        _configuration.GetSection("AppSettings:Token").Value!));

    // Anahtarı kullanarak imzalama algoritmasını seç
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

    // Token özelliklerini belirle
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1), // Token 1 gün geçerli olsun
        SigningCredentials = creds
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return tokenHandler.WriteToken(token);
}
}