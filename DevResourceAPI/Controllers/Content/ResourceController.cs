using Microsoft.AspNetCore.Mvc;
using DevResourceAPI.Models;
using DevResourceAPI.Services;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using DevResourceAPI.Extensions; //  Extension metodumuzu buraya ekledik

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
    public async Task<IActionResult> GetResources(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // Extension sayesinde tek satırda ID alıyoruz (Giriş yapmadıysa 0 döner)
        // Eğer 0 ise (giriş yapmamışsa) null gönderiyoruz ki servis herkese açık verileri getirsin.
        int userId = User.GetUserId();
        int? queryUserId = userId == 0 ? null : userId;

        var result = await _resourceService.GetResourcesAsync(
            categoryId, search, pageNumber, pageSize);

        if (!result.Success) return BadRequest(result);

        return Ok(new 
        { 
            TotalRecords = result.Data!.TotalRecords,
            Data = result.Data.Items 
        });
    }

    [HttpGet("{id}")]
public async Task<ActionResult<ResourceDto>> GetResourceById(int id)
{
    var result = await _resourceService.GetResourceByIdAsync(id);
    if (!result.Success) return NotFound(new { message = "Kaynak bulunamadı." });

    var resource = result.Data!;

    // Entity -> DTO Dönüşümü (Mapping)
    var resourceDto = new ResourceDto
    {
        Id = resource.Id,
        Title = resource.Title,
        Url = resource.Url,
        Description = resource.Description ?? "",
        CategoryId = resource.CategoryId,
        CategoryName = resource.Category != null ? resource.Category.Name : "Genel",
        OwnerName = resource.User != null ? resource.User.UserName! : "Bilinmiyor", // Null check eklendi
        CreatedBy = resource.User != null ? resource.User.UserName! : "Bilinmiyor",
        CreatedAt = resource.CreatedAt
    };

    return Ok(resourceDto);
}

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceDto request)
    {
        //  TEMİZLİK 1: Extension Kullanımı
        var userId = User.GetUserId();
        var userName = User.Identity?.Name ?? "Kullanıcı";

        // Not: İleride AutoMapper ile burayı da tek satıra düşüreceğiz.
        var resource = new Resource
        {
            Title = request.Title,
            Url = request.Url,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        //  TEMİZLİK 2: Try-Catch bloğu kalktı (GlobalExceptionMiddleware halledecek)
        var result = await _resourceService.CreateResourceAsync(resource, userId);

        if (!result.Success) return BadRequest(new { message = result.Message });

        var createdData = result.Data!; 

        // Response DTO Hazırlığı
        var returnDto = new ResourceDto
        {
            Id = createdData.Id,
            Title = createdData.Title,
            Description = createdData.Description ?? "",
            Url = createdData.Url,
            CategoryId = createdData.CategoryId,
            CategoryName = "Yeni Eklendi", // Bunu Service'den dolu getirmek daha doğrudur
            OwnerName = userName
        };

        return Ok(returnDto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateResource(int id, [FromBody] Resource resource)
    {
        //  TEMİZLİK: Tekrarlayan kodlar gitti, Extension geldi
        var result = await _resourceService.UpdateResourceAsync(
            id, 
            resource, 
            User.GetUserId(), 
            User.GetUserRole()
        );

        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> PatchResource(int id, [FromBody] JsonPatchDocument<Resource> patchDoc)
    {
        if (patchDoc == null) return BadRequest();

        //  TEMİZLİK: Tekrarlayan kodlar gitti
        var result = await _resourceService.PatchResourceAsync(
            id, 
            patchDoc, 
            User.GetUserId(), 
            User.GetUserRole()
        );

        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteResource(int id)
    {
        //  TEMİZLİK: Tekrarlayan kodlar gitti
        var result = await _resourceService.DeleteResourceAsync(
            id, 
            User.GetUserId(), 
            User.GetUserRole()
        );

        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }
}