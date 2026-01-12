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
    // Dönüş tipini ActionResult<Resource> yerine ActionResult<ResourceDto> yap (Opsiyonel ama şık durur)
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userName = User.Identity?.Name ?? "Kullanıcı"; // Token'dan ismini alıyoruz

        // DTO -> Entity Çevrimi
        var resource = new Resource
        {
            Title = request.Title,
            Url = request.Url,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        try 
        {
            // Servis veritabanına kaydeder ve Entity döner
            var createdResource = await _resourceService.CreateResourceAsync(resource, userId);

            // --- HATAYI ÇÖZEN KISIM ---
            // Entity'i (createdResource) direkt döndürme! Sonsuz döngü yapar.
            // Onun yerine temiz bir DTO oluşturup onu döndür.
            
            var returnDto = new ResourceDto
            {
                Id = createdResource.Id,
                Title = createdResource.Title,
                Url = createdResource.Url,
                CategoryName = "Yeni Eklendi", // Kayıt anında tekrar DB'ye sormamak için statik yazabiliriz
                OwnerName = userName // Token'dan aldığımız isim
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
    public async Task<IActionResult> UpdateResource(int id, [FromBody] UpdateResourceDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value?? "User";
        // Manuel Mapping (DTO -> Entity)
        var resource = new Resource
        {
            Id = id,
            Title = request.Title,
            Url = request.Url,
            CategoryId = request.CategoryId,
           // UserId'yi serviste 'existing' kayıttan koruyoruz, buraya yazmaya gerek yok
        };

        var (success, message) = await _resourceService.UpdateResourceAsync(id, resource, userId, userRole);
        
        if (!success) return StatusCode(403, new { message });
        return Ok(new { message });
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