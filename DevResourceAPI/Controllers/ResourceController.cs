using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    
using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch;

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

[HttpGet]

public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResources(string? searchTerm)
{
   
    // 1. IQueryable kullanarak sorguyu "hazırla" (Henüz veritabanına gitmedik)
    var query = _context.Resources.AsQueryable();

    // 2. Filtreleme Mantığı (Sadece arama kelimesi varsa çalışır)
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        searchTerm = searchTerm.ToLower();
        query = query.Where(r => r.Title.ToLower().Contains(searchTerm) 
                              || r.Description.ToLower().Contains(searchTerm));
    }

    // 3. Sadece ihtiyacımız olan kolonları SELECT et (Performans için önemli)
    // Bu işlem SQL'deki "SELECT Title, Url FROM..." komutuna dönüşür.
    var result = await query
        .Select(r => new ResourceDto
        {
            Id = r.Id,
            Title = r.Title,
            Url = r.Url,
            // Include yapmaya gerek kalmadan doğrudan isme ulaşıyoruz
            CategoryName = r.Category != null ? r.Category.Name : "Kategori Tanımsız"
        })
        .ToListAsync(); // Veritabanına şuan tek bir optimize SQL sorgusu gitti.

    return Ok(result);
}

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