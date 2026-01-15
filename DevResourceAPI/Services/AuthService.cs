using DevResourceAPI.DTOs;
using DevResourceAPI.Data;
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
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, UserManager<User> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<ServiceResult> RegisterAsync(UserRegisterDto request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser != null)
            return ServiceResult.Fail("Kullanıcı adı zaten alınmış.");

        var newUser = new User
        {
            UserName = request.UserName,
            CreatedAt = DateTime.UtcNow,
            Role = "User" // Varsayılan rol
           
        };

        var result = await _userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
            return ServiceResult.Fail("Kayıt başarısız: " + string.Join(", ", result.Errors.Select(e => e.Description)));

        // Varsayılan rol ataması (Önemli)
        await _userManager.AddToRoleAsync(newUser, "User");

        return ServiceResult.Ok("Kayıt başarılı.");
    }

    public async Task<ServiceResult<string>> LoginAsync(UserLoginDto request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null) 
            return ServiceResult<string>.Fail("Kullanıcı bulunamadı.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return ServiceResult<string>.Fail("Şifre yanlış.");

        var token = GenerateJwtToken(user);
        return ServiceResult<string>.Ok(token);
    }

    // ✅ DÜZELTİLEN KISIM: Sayfalama Parametreleri ve totalRecords eklendi
    public async Task<ServiceResult<PagedResult<UserDto>>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        var query = _userManager.Users.AsQueryable();

        // 1. Toplam sayıyı hesapla (Hata veren totalRecords buydu)
        var totalRecords = await query.CountAsync();

        // 2. Sırala
        query = query.OrderByDescending(u => u.CreatedAt);

        // 3. Sayfala
        if (pageSize > 0)
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        // 4. Veriyi Çek ve DTO'ya dönüştür
        var users = await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                Role = u.Role ?? "User", // Role null gelirse varsayılan ata
                CreatedAt = u.CreatedAt 
            })
            .ToListAsync();

        // 5. Paketle
        var pagedData = new PagedResult<UserDto>(users, totalRecords);
        return ServiceResult<PagedResult<UserDto>>.Ok(pagedData);
    }
    public async Task<ServiceResult> DeleteAccountAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return ServiceResult.Fail("Kullanıcı bulunamadı.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded 
            ? ServiceResult.Ok("Hesap silindi.") 
            : ServiceResult.Fail("Silinemedi.");
    }

    public async Task<ServiceResult> DeleteUserByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return ServiceResult.Fail("Kullanıcı bulunamadı.");

        // Admin koruması
        if (user.Role == "Manager") return ServiceResult.Fail("Yöneticiler silinemez.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded 
            ? ServiceResult.Ok("Kullanıcı silindi.") 
            : ServiceResult.Fail("Silinemedi.");
    }

    public async Task<ServiceResult<SystemStatsDto>> GetSystemStatsAsync()
    {
        var stats = new SystemStatsDto
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalResources = await _context.Resources.CountAsync(),
            TotalCategories = await _context.Categories.CountAsync(),
            LastUpdate = DateTime.UtcNow 
        };

        return ServiceResult<SystemStatsDto>.Ok(stats);
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

        // Token key kontrolü
        var keyString = _configuration["AppSettings:Token"]; 
        if(string.IsNullOrEmpty(keyString)) keyString = _configuration["Jwt:Key"]; // Yedek kontrol

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString!));
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