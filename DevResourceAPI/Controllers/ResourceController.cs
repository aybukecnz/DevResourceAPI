using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    
using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;

namespace DevResourceAPI.Controllers;   
[ApiController]
[Route("api/[controller]")] // Adres: api/category olacak
public class ResourceController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructor: Veritabanı bağlantısını sisteme enjekte ediyoruz 
    public ResourceController(AppDbContext context)
    {
        _context = context;
    }
    [AllowAnonymous]
    [HttpGet]
public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResources(
    [FromQuery] string? searchTerm,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
{  
    var query = _context.Resources.AsQueryable();

    // 1. Filtreleme (Arama)
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        searchTerm = searchTerm.ToLower();
        query = query.Where(r => 
            (r.Title != null && r.Title.ToLower().Contains(searchTerm)) || 
            (r.Description != null && r.Description.ToLower().Contains(searchTerm)) || 
            (r.Category != null && r.Category.Name != null && r.Category.Name.ToLower().Contains(searchTerm)));
    }

    // 2. Bütünleşik Paging ve Select İşlemi
    // önce toplam kayıt sayısını al
    var totalRecords = await query.CountAsync();
    var result = await query
        .OrderBy(r => r.Id) // ID'ye göre artan
        .Skip((pageNumber - 1) * pageSize) // Kaçıncı sayfadaysak o kadar atla
        .Take(pageSize)                   // Sadece istenen sayfa boyutu kadar al
        .Select(r => new ResourceDto      // Sadece ihtiyacımız olan kolonları çek
        {
            Id = r.Id,
            Title = r.Title,
            Url = r.Url,
            CategoryName = r.Category != null ? r.Category.Name : "Kategori Tanımsız"
        })
        .ToListAsync(); 
// sadece listeye değil, toplam kayıt sayısına da ihtiyacım var
    return Ok(new
    {
        TotalRecords = totalRecords,
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling((double)totalRecords / (double)pageSize),
        Data = result
    });
}

    [Authorize]
    // POST: api/resource (Yeni bir kaynak/link ekle)
    [HttpPost]
public async Task<ActionResult<Resource>> CreateResource(Resource resource)
{
    // 1. Kategori var mı? (AnyAsync kullanımı çok performanslıdır)
    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == resource.CategoryId);
    if (!categoryExists)
    {
        return BadRequest("Hata: Belirttiğiniz kategori ID'si veritabanında bulunamadı.");
    }

    // 2. Bu link daha önce eklenmiş mi? (Veri kirliliğini önlemek için şart)
    var urlExists = await _context.Resources.AnyAsync(r => r.Url == resource.Url);
    if (urlExists)
    {
        return BadRequest("Hata: Bu URL zaten kütüphanede kayıtlı.");
    }

    _context.Resources.Add(resource);
    await _context.SaveChangesAsync();
    
    return Ok(resource);
}
    [Authorize]
// PUT: api/resource/5 (Var olan bir kaynağı güncelle)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResource(int id, Resource resource)
    {
        // Güvenlik Kontrolü: URL'deki ID ile gönderilen nesnedeki ID uyuşuyor mu?
        if (id != resource.Id)
        {
            return BadRequest("Hata: ID uyuşmazlığı.");
        }

        // Kategori var mı kontrolü (Güncellerken olmayan bir kategoriye atamasınlar)
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == resource.CategoryId);
        if (!categoryExists)
        {
            return BadRequest("Hata: Belirttiğiniz yeni kategori ID'si bulunamadı.");
        }

        // Entity Framework'e bu nesnenin değiştirildiğini söylüyoruz
        _context.Entry(resource).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Resources.AnyAsync(e => e.Id == id))
            {
                return NotFound("Hata: Güncellemek istediğiniz kaynak bulunamadı.");
            }
            throw;
        }

        return NoContent(); // 204 No Content: İşlem başarılı, dönecek veri yok.
    }
    [Authorize]
    // DELETE: api/resource/5 (Bir kaynağı sil)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound("Hata: Silinmek istenen kaynak bulunamadı.");
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{id} ID'li kaynak başarıyla silindi." });
    }
    [Authorize]
    [HttpPatch("{id}")]
public async Task<IActionResult> PatchResource(int id, [FromBody] JsonPatchDocument<Resource> patchDoc)
{
    if (patchDoc == null) return BadRequest();

    var resource = await _context.Resources.FindAsync(id);
    if (resource == null) return NotFound("Kaynak bulunamadı.");

    // Değişiklikleri mevcut nesneye uygula
    patchDoc.ApplyTo(resource, ModelState);

    // Uygulama sonrası model doğrulaması yap
    if (!TryValidateModel(resource)) return BadRequest(ModelState);

    await _context.SaveChangesAsync();
    return NoContent();
}
}