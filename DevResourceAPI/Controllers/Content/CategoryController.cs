using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.Services;
using DevResourceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DevResourceAPI.DTOs;


namespace DevResourceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _categoryService.GetCategoryAsync(search, pageNumber, pageSize);

        if (!result.Success) return BadRequest(result);

        return Ok(new 
        { 
            TotalRecords = result.Data!.TotalRecords,
            Data = result.Data.Items 
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userName = User.Identity?.Name ?? "Kullanıcı";

        var category = new Category { Name = request.Name, UserId = userId };

        var result = await _categoryService.CreateCategoryAsync(category, userId);

        if (!result.Success) return BadRequest(new { message = result.Message });

        var createdCategory = result.Data!;

        return Ok(new CategoryDto
        {
            Id = createdCategory.Id,
            Name = createdCategory.Name,
            OwnerName = userName
        });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        var result = await _categoryService.UpdateCategoryAsync(id, new Category { Name = request.Name }, userId, userRole);

        if (!result.Success) return StatusCode(403, new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(int id, [FromQuery] bool confirm = false)
    {
        if (!confirm) return BadRequest(new { message = "Onay (?confirm=true) gerekli." });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var result = await _categoryService.DeleteCategoryAsync(id, userId, userRole);

        if (!result.Success) return StatusCode(403, new { message = result.Message });
        return Ok(new { message = result.Message });
    }
}