using DevResourceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using DevResourceAPI.Attributes;
namespace DevResourceAPI.Controllers;
[ApiController]
[Route("api/[controller]")]  // Address: api/category olacak
[ApiKey] // Bu endpoint'e erişim için API anahtarı zorunlu
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ApiKey] 
    public async Task<IActionResult> Register([FromBody] UserDto userDto)
    {
        var user = await _authService.Register(userDto.Username, userDto.Password);
        if (user == null)
            return BadRequest("Kullanıcı zaten mevcut.");

        return Ok("Kayıt başarılı.");
    }
  
    [HttpPost("login")]
    [ApiKey]
    public async Task<IActionResult> Login([FromBody] UserDto userDto)
    {
        var user = await _authService.Login(userDto.Username, userDto.Password);
        if (user == null)
            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");

        // Giriş başarılıysa artık "Giriş başarılı" yerine TOKEN dönüyoruz
    var token = _authService.CreateToken(user);
    return Ok(new { token = token });
    }
    [HttpGet("me")]
    [Authorize] // Sadece giriş yapanlar görebilir
    public async Task<IActionResult> GetMyProfile()
{
    // 1. Token'dan User ID'yi al
    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    if (userIdClaim == null) return Unauthorized();

    int userId = int.Parse(userIdClaim.Value);

    // 2. Veritabanından kullanıcıyı bul
    var user = await _authService.GetProfileAsync(userId);
    if (user == null) return NotFound("Kullanıcı bulunamadı.");
    return Ok(user);
}
    
    [HttpDelete("delete-account")]
    [ApiKey]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] UserDto userDto)
    {
    // Senin yazdığın AuthService içindeki DeleteUser metodunu çağırıyoruz
    var result = await _authService.DeleteUser(userDto.Username, userDto.Password);
    
    if (!result)
    {
        // Şifre yanlışsa veya kullanıcı yoksa hata dön
        return BadRequest("Kullanıcı adı veya şifre hatalı.");
    }

    return Ok("Hesabınız başarıyla silindi.");
}
}
