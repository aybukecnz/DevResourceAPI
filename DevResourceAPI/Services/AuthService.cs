using DevResourceAPI.DTOs;
using DevResourceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DevResourceAPI.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(UserRegisterDto request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
            return (false, "Bu kullanıcı adı zaten alınmış.");

        var newUser = new User
        {
            UserName = request.Username, // Büyük 'N' ile (Düzeltme)
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
            return (false, "Kayıt başarısız: " + string.Join(", ", result.Errors.Select(e => e.Description)));

        return (true, "Kayıt başarılı.");
    }

    public async Task<(bool Success, string Token)> LoginAsync(UserLoginDto request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null) return (false, "Kullanıcı bulunamadı.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return (false, "Şifre yanlış.");

        return (true, GenerateJwtToken(user));
    }

    // 👇 GERİ EKLENEN ÖZELLİK: Hesap Silme
    public async Task<(bool Success, string Message)> DeleteAccountAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return (false, "Kullanıcı bulunamadı.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded ? (true, "Hesap silindi.") : (false, "Silinemedi.");
    }

    // 👇 GERİ EKLENEN ÖZELLİK: Tüm Kullanıcıları Listeleme
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        // Burada sadece 'Username' -> 'UserName' değişikliği yaptım, kodun aynen duruyor.
        return await _userManager.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.UserName! // Burası hatayı çözen kısım
            })
            .ToListAsync();
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? "")
        };

        if (!string.IsNullOrEmpty(user.Role)) 
            claims.Add(new Claim(ClaimTypes.Role, user.Role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // 👇 GERİ EKLENEN ÖZELLİK: Kullanıcıyı ID ile Silme (Banlama)
    public async Task<(bool Success, string Message)> DeleteUserByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return (false, "Kullanıcı bulunamadı.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded ? (true, "Kullanıcı silindi.") : (false, "Silinemedi.");
    }
}