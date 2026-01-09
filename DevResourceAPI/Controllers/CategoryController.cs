using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.Services;
using DevResourceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await _categoryService.GetAllCategoriesAsync());
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _categoryService.CreateCategoryAsync(category, userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(int id, [FromQuery] bool confirm = false)
    {
        if (!confirm) return BadRequest(new { message = "Onay (?confirm=true) gerekli." });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var (success, message) = await _categoryService.DeleteCategoryAsync(id, userId, userRole);

        if (!success) return StatusCode(403, new { message });
        return Ok(new { message });
    }
}