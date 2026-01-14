using DevResourceAPI.DTOs;
using DevResourceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public async Task<ServiceResult> RegisterAsync(UserRegisterDto request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
            return ServiceResult.Fail("Kullanıcı adı zaten alınmış.");

        var newUser = new User
        {
            UserName = request.Username, // Büyük 'N' ile (Düzeltme)
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true 
        };

        var result = await _userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
            return ServiceResult.Fail("Kayıt başarısız: " + string.Join(", ", result.Errors.Select(e => e.Description)));

        return ServiceResult.Ok("Kayıt başarılı.");
    }

    public async Task<ServiceResult<string>> LoginAsync(UserLoginDto request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null) 
        return ServiceResult<string>.Fail("Kullanıcı bulunamadı.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return ServiceResult<string>.Fail("Şifre yanlış.");

        var token = GenerateJwtToken(user);
        return ServiceResult<string>.Ok(token, "Giriş başarılı.");
    }

    // Hesap Silme
    public async Task<ServiceResult> DeleteAccountAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) 
        return ServiceResult.Fail("Kullanıcı bulunamadı.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded 
        ? ServiceResult.Ok("Hesap silindi.") 
        : ServiceResult.Fail("Silinemedi.");
    }

    // Tüm Kullanıcıları Listeleme
    public async Task<ServiceResult<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.UserName! 
            })
            .ToListAsync();
        return ServiceResult<IEnumerable<UserDto>>.Ok(users);
    }
     // Kullanıcıyı ID ile Silme (Banlama)
    public async Task<ServiceResult> DeleteUserByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return ServiceResult.Fail("Kullanıcı bulunamadı.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded 
        ? ServiceResult.Ok("Kullanıcı silindi.") 
        : ServiceResult.Fail("Silinemedi.");
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
}