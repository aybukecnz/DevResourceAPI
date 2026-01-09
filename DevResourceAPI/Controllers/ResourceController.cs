using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.Services;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;

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
    [AllowAnonymous]
    public async Task<IActionResult> GetResources([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _resourceService.GetAllResourcesAsync(searchTerm, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Resource>> CreateResource(Resource resource)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _resourceService.CreateResourceAsync(resource, userId);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateResource(int id, Resource resource)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var (success, message) = await _resourceService.UpdateResourceAsync(id, resource, userId, userRole);
        
        if (!success) return StatusCode(403, new { message });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var (success, message) = await _resourceService.DeleteResourceAsync(id, userId, userRole);

        if (!success) return StatusCode(403, new { message });
        return Ok(new { message });
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> PatchResource(int id, [FromBody] JsonPatchDocument<Resource> patchDoc)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var (success, message) = await _resourceService.PatchResourceAsync(id, patchDoc, userId, userRole, ModelState);

        if (!success) return StatusCode(403, new { message });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        return NoContent();
    }
}