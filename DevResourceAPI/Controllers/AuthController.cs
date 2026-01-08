using DevResourceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.DTOs;

namespace DevResourceAPI.Controllers;
[ApiController]
[Route("api/[controller]")]  // Adres: api/category olacak

public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto userDto)
    {
        var user = await _authService.Register(userDto.Username, userDto.Password);
        if (user == null)
            return BadRequest("Kullanıcı zaten mevcut.");

        return Ok("Kayıt başarılı.");
    }
  
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDto userDto)
    {
        var user = await _authService.Login(userDto.Username, userDto.Password);
        if (user == null)
            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");

        // Giriş başarılıysa artık "Giriş başarılı" yerine TOKEN dönüyoruz
    var token = _authService.CreateToken(user);
    return Ok(new { token = token });
    }
  
    [HttpDelete("delete-account")]
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
