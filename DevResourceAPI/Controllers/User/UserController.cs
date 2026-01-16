using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevResourceAPI.Services;
using DevResourceAPI.DTOs;

namespace DevResourceAPI.Controllers.Users; 

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    // HERKES (User + Manager)
    // Kendi profil bilgilerini getir
    // GET: api/user/me
    [HttpGet("me")]
    public IActionResult GetMyProfile()
    {
        // Token'dan bilgileri okuyabilirsin
        var username = User.Identity?.Name;
        return Ok(new { Message = $"Merhaba {username}, bu senin profilin." });
    }

    // SADECE YÖNETİCİ (Manager)
    // Tüm kullanıcıları listele
    // GET: api/user
    [HttpGet]
    [Authorize(Roles = "Manager")] 
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _authService.GetAllUsersAsync(pageNumber, pageSize);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // SADECE YÖNETİCİ (Manager)
    // Bir kullanıcıyı sil
    // DELETE: api/user/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")] 
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _authService.DeleteUserByIdAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}