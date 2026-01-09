using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net; 
using DevResourceAPI.Data;
using DevResourceAPI.DTOs;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DevResourceAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // --- KAYIT OL ---
    public async Task<(bool Success, string Message)> RegisterAsync(UserRegisterDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return (false, "Bu kullanıcı adı zaten alınmış.");

        /* * [SAVUNMA SANAYİİ NOTU]
         * Projede şu an pratikliği ve Web endüstri standardı olması nedeniyle "BCrypt" kütüphanesi tercih edilmiştir.
         * Ancak Savunma Sanayii veya kritik altyapı projelerinde (ASELSAN, HAVELSAN vb.):
         * 1. Tedarik Zinciri Saldırılarını (Supply Chain Attacks) önlemek için dış kütüphane bağımlılığını sıfırlamak,
         * 2. FIPS/NIST standartlarına (Native .NET Implementation) tam uyum sağlamak adına,
         * İleride .NET'in yerleşik 'System.Security.Cryptography' (HMACSHA512/PBKDF2) yapısına geçiş yapılmalıdır.
         */
        
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash, 
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "Kayıt başarılı.");
    }

    // --- GİRİŞ YAP ---
    public async Task<(bool Success, string Message, string? Token)> LoginAsync(UserLoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null)
            return (false, "Kullanıcı bulunamadı.", null);

        // Şifre Doğrulama
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (false, "Şifre hatalı.", null);

        // Token Üret
        string token = CreateToken(user);

        return (true, "Giriş başarılı.", token);
    }

    // --- HESAP SİLME (YENİ) ---
    public async Task<(bool Success, string Message)> DeleteAccountAsync(int userId, string password)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (false, "Kullanıcı bulunamadı.");

        // Güvenlik: Silmeden önce şifresini tekrar doğruluyoruz!
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Şifre yanlış. Hesabınızı silmek için şifrenizi doğru girmelisiniz.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return (true, "Hesabınız başarıyla silindi.");
    }

    // --- TOKEN OLUŞTURUCU ---
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Token").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}