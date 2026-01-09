using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.DTOs;
using DevResourceAPI.Services;
using Microsoft.AspNetCore.Authorization; // Kilit mekanizması için şart
using System.Security.Claims; // Token içinden ID okumak için şart

namespace DevResourceAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // --- KAYIT OL (Herkese Açık) ---
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register(UserRegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    // --- GİRİŞ YAP (Herkese Açık) ---
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Login(UserLoginDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { token = result.Token, message = result.Message });
    }

    // --- HESABIMI SİL (Kilitli - Sadece Giriş Yapan) ---
    [HttpDelete("delete-my-account")]
    [Authorize] 
    public async Task<IActionResult> DeleteMyAccount([FromQuery] string password)
    {
        // 1. Token'dan kullanıcının ID'sini çekiyoruz.
        // Böylece kullanıcı "Ahmet" iken "Mehmet"in ID'sini gönderip onu silemez.
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized(new { message = "Kimlik doğrulanamadı." });

        int userId = int.Parse(userIdString);

        // 2. Servise gönderiyoruz
        var result = await _authService.DeleteAccountAsync(userId, password);

        if (!result.Success) 
            return BadRequest(new { message = result.Message });
            
        return Ok(new { message = result.Message });
    }
}