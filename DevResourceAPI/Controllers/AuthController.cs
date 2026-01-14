using DevResourceAPI.DTOs;
using DevResourceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        // Service artık (Success, Token) dönüyor.
        // Eğer başarısızsa 'Token' değişkeni hata mesajını taşıyor.
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            // Başarısızsa result.Token aslında hata mesajıdır (Service öyle ayarlandı)
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("delete")]
    [Authorize] //  Sadece giriş yapmış kullanıcı silebilir
    public async Task<IActionResult> DeleteAccount()
    {
        // Kullanıcı adını Token'dan alıyoruz (Güvenli yöntem)
        var username = User.Identity?.Name;

        if (string.IsNullOrEmpty(username))
            return Unauthorized("Kullanıcı bulunamadı.");

        // Artık sadece username gönderiyoruz (Service böyle istiyor)
        var result = await _authService.DeleteAccountAsync(username);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}