using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevResourceAPI.Services;
using DevResourceAPI.Data; 
using Microsoft.EntityFrameworkCore;

namespace DevResourceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
// DİKKAT: Sadece "Manager" rolü olanlar bu kapıdan geçebilir.
[Authorize(Roles = "Manager")] 
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    public AdminController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    // TÜM KULLANICILARI LİSTELE 
    // GET: api/Admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    // KULLANICIYI BANLA / SİL 
    // DELETE: api/Admin/users/5
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> BanUser(int id)
    {
        var result = await _authService.DeleteUserByIdAsync(id);
        
        if (!result.Success) return BadRequest(new { message = result.Message });
        
        return Ok(new { message = result.Message });
    }

    // GENEL İSTATİSTİKLER 
    // GET: api/Admin/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        // Basitçe veritabanındaki sayıları çek
        var totalUsers = await _context.Users.CountAsync();
        var totalResources = await _context.Resources.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();

        return Ok(new 
        { 
            TotalUsers = totalUsers, 
            TotalResources = totalResources, 
            TotalCategories = totalCategories,
            Message = "Sistem durumu stabil patron!"
        });
    }
}