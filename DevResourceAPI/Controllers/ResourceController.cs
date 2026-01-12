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

    // GET: api/Resource
    [HttpGet]
    public async Task<IActionResult> GetAllResources(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // İşte hatayı çözen kısım burası: Parametreleri eksiksiz gönderiyoruz
        var result = await _resourceService.GetAllResourcesAsync(search, categoryId, pageNumber, pageSize);

        return Ok(new 
        { 
            TotalRecords = result.TotalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(result.TotalRecords / (double)pageSize),
            Data = result.Data
        });
    }

    // GET: api/Resource/grouped (YENİ ÖZELLİK)
    [HttpGet("grouped")]
    public async Task<ActionResult<IEnumerable<UserGroupedResourceDto>>> GetGroupedResources()
    {
        var groupedResources = await _resourceService.GetAllResourcesGroupedAsync();
        return Ok(groupedResources);
    }

    // GET: api/Resource/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Resource>> GetResourceById(int id)
    {
        var resource = await _resourceService.GetResourceByIdAsync(id);
        if (resource == null) return NotFound(new { message = "Kaynak bulunamadı." });
        return Ok(resource);
    }

    // POST: api/Resource
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
            Description = request.Description, // Açıklamayı unutmuyoruz
            CategoryId = request.CategoryId,
            UserId = userId
        };

        try 
        {
            var createdResource = await _resourceService.CreateResourceAsync(resource, userId);

            var returnDto = new ResourceDto
            {
                Id = createdResource.Id,
                Title = createdResource.Title,
                Description = createdResource.Description ?? "",
                Url = createdResource.Url,
                CategoryId = createdResource.CategoryId,
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

    // PUT: api/Resource/5
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

    // PATCH: api/Resource/5
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

    // DELETE: api/Resource/5
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