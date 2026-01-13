using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.Services;
using DevResourceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DevResourceAPI.DTOs;
using DevResourceAPI.Models.Common;

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
    public async Task<IActionResult> GetAllCategories(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _categoryService.GetAllCategoriesAsync(search, pageNumber, pageSize);

        return Ok(new 
        { 
            TotalRecords = result.TotalRecords,
            Data = result.Data 
        });
    }

    [HttpPost]
    [Authorize]
    // 1. Dönüş tipini 'ActionResult<Category>' yerine 'ActionResult<CategoryDto>' yap
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userName = User.Identity?.Name ?? "Kullanıcı"; // Token'dan isim al

        // DTO -> Entity
        var category = new Category
        {
            Name = request.Name,
            UserId = userId
        };

        // Servise kaydettir
        var createdCategory = await _categoryService.CreateCategoryAsync(category, userId);

        // Entity -> DTO Çevrimi (Temiz Dönüş İçin)
        var returnDto = new CategoryDto
        {
            Id = createdCategory.Id,
            Name = createdCategory.Name,
            OwnerName = userName // Token'daki ismi basıyoruz
        };

        return Ok(returnDto);
    }
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        // DTO -> Entity Dönüşümü (Mapping)
        // Burada sadece ismi al, güvenli.
        var category = new Category
        {
            Name = request.Name
        };

        var (success, message) = await _categoryService.UpdateCategoryAsync(id, category, userId, userRole);

        if (!success) return StatusCode(403, new { message });
        return Ok(new { message });
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