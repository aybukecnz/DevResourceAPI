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
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _authService.GetAllUsersAsync(pageNumber, pageSize);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    // KULLANICIYI BANLA / SİL 
    // DELETE: api/Admin/users/5
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> BanUser(int id)
    {
        var result = await _authService.DeleteUserByIdAsync(id);
        
        if (!result.Success) return BadRequest(result);
        
        return Ok(result);
    }

    // GENEL İSTATİSTİKLER 
    // GET: api/Admin/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
{
    // Veritabanına gitmeme gerek yok, Servis benim için hazırladı bile!
    var result = await _authService.GetSystemStatsAsync();
    
    return Ok(result);
}
}