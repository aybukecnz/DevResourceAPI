using DevResourceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevResourceAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")] // Sadece Admin rolündeki kullanıcılar erişebilir
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _userService.GetAllUsersAsync(search, pageNumber, pageSize);

        return Ok(new 
        { 
            TotalRecords = result.TotalRecords,
            Data = result.Data 
        });
    }
}