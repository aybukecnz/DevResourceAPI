using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevResourceAPI.Services;
using System.Security.Claims;

namespace DevResourceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Sadece giriş yapmış kullanıcılar beğenebilir/takip edebilir
public class SocialController : ControllerBase
{
    private readonly ISocialService _socialService;

    public SocialController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    // POST: api/Social/like/5
    [HttpPost("like/{resourceId}")]
    public async Task<IActionResult> LikeResource(int resourceId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var result = await _socialService.ToggleResourceLikeAsync(resourceId, userId);
        
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // POST: api/Social/follow/3
    [HttpPost("follow/{username}")]
    public async Task<IActionResult> FollowUser(string username)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _socialService.ToggleUserFollowAsync(username, currentUserId);

        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }
}