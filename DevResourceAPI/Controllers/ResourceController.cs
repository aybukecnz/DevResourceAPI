using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    
using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DevResourceAPI.Controllers;   

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourceController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Herkes görebilir
    [HttpGet]
    public async Task<ActionResult> GetResources([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Resources.Include(r => r.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(r => 
                (r.Title != null && r.Title.ToLower().Contains(searchTerm)) || 
                (r.Category != null && r.Category.Name != null && r.Category.Name.ToLower().Contains(searchTerm)));
        }

        var totalRecords = await query.CountAsync();
        var result = await query
            .OrderBy(r => r.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ResourceDto {
                Id = r.Id,
                Title = r.Title,
                Url = r.Url,
                CategoryName = r.Category != null ? r.Category.Name : "Kategori Tanımsız"
            }).ToListAsync();

        return Ok(new { TotalRecords = totalRecords, Data = result });
    }

    // POST: Kendi adına ekleme
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Resource>> CreateResource(Resource resource)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        resource.UserId = userId; // Sahiplik atandı

        if (!await _context.Categories.AnyAsync(c => c.Id == resource.CategoryId))
            return BadRequest(new { message = "Geçersiz Kategori ID." });

        if (await _context.Resources.AnyAsync(r => r.Url == resource.Url))
            return BadRequest(new { message = "Bu URL zaten kayıtlı." });

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return Ok(resource);
    }

    // PUT: Sahibi veya Manager
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateResource(int id, Resource resource)
    {
        var existingResource = await _context.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (existingResource == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (existingResource.UserId != currentUserId && currentUserRole != "Manager")
            return StatusCode(403, new { message = "Yetkiniz yok." });

        resource.Id = id;
        resource.UserId = existingResource.UserId;
        _context.Entry(resource).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: Sahibi veya Manager
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return StatusCode(403, new { message = "Yetkiniz yok." });

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Kaynak silindi." });
    }

    // PATCH: Sahibi veya Manager
    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> PatchResource(int id, [FromBody] JsonPatchDocument<Resource> patchDoc)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return StatusCode(403, new { message = "Yetkiniz yok." });

        patchDoc.ApplyTo(resource, ModelState);
        if (!TryValidateModel(resource)) return BadRequest(ModelState);

        await _context.SaveChangesAsync();
        return NoContent();
    }
}