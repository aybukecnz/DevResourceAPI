using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevResourceAPI.Services;
using DevResourceAPI.Attributes;

namespace DevResourceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager")] 
public class DashboardController : ControllerBase
{
    private readonly IAuthService _authService;

    public DashboardController(IAuthService authService)
    {
        _authService = authService;
    }

    // Sistemin genel özetini getirir (Kullanıcı sayısı, Kaynak sayısı vb.)
    // Adres: GET api/dashboard/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        // AuthService içindeki metodu çağırıyoruz
        var result = await _authService.GetSystemStatsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
    [HttpGet("logs")]
    public async Task<IActionResult> GetErrorLogs()
    {
        var result = await _authService.GetErrorLogsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
        
      
    
}