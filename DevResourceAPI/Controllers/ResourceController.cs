using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    
using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;

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

    // 2. Filtreleme (Sorgu hala SQL'e dönüşme aşamasında)
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var lowerSearch = searchTerm.ToLower();
        query = query.Where(r => r.Title.ToLower().Contains(lowerSearch) 
                              || r.Description.ToLower().Contains(lowerSearch));
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
}}