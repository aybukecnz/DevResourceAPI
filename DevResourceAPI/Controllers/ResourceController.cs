using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.Models;
using DevResourceAPI.Services;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.JsonPatch;

namespace DevResourceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllResources(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? userId,        
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        int? currentUserId = null;
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId != null) currentUserId = int.Parse(claimId.Value);
        }

        // Servisten PagedResult dönüyor
        var result = await _resourceService.GetAllResourcesAsync(
            search, categoryId, userId, pageNumber, pageSize, currentUserId);

        if (!result.Success) return BadRequest(result);

        // Enterprise Erişim: .Data.Items ve .Data.TotalRecords
        return Ok(new 
        { 
            TotalRecords = result.Data!.TotalRecords,
            Data = result.Data.Items // <-- BURASI DEĞİŞTİ (.Items oldu)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Resource>> GetResourceById(int id)
    {
        var result = await _resourceService.GetResourceByIdAsync(id);
        if (!result.Success) return NotFound(new { message = "Kaynak bulunamadı." });
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userName = User.Identity?.Name ?? "Kullanıcı";

        var resource = new Resource
        {
            Title = request.Title,
            Url = request.Url,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        try 
        {
            var result = await _resourceService.CreateResourceAsync(resource, userId);

            if (!result.Success) return BadRequest(new { message = result.Message });

            var createdData = result.Data!; 

            var returnDto = new ResourceDto
            {
                Id = createdData.Id,
                Title = createdData.Title,
                Description = createdData.Description ?? "",
                Url = createdData.Url,
                CategoryId = createdData.CategoryId,
                CategoryName = "Yeni Eklendi",
                OwnerName = userName
            };

            return Ok(returnDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateResource(int id, [FromBody] Resource resource)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        var result = await _resourceService.UpdateResourceAsync(id, resource, userId, userRole);

        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> PatchResource(int id, [FromBody] JsonPatchDocument<Resource> patchDoc)
    {
        if (patchDoc == null) return BadRequest();

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        var result = await _resourceService.PatchResourceAsync(id, patchDoc, userId, userRole);

        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        var result = await _resourceService.DeleteResourceAsync(id, userId, userRole);

        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }
}