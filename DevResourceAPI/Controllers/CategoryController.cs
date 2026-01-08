using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace DevResourceAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // Adres: api/category olacak
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructor: Veritabanı bağlantısını sisteme enjekte ediyoruz 
    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/category (Tüm kategorileri getir)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    // POST: api/category (Yeni kategori ekle)
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(category);
    }
    // DELETE: api/category/{id} (Kategori sil)
    [HttpDelete("{id}")]
    [Authorize] // Sadece giriş yapmış user silebilir
    public async Task<IActionResult> DeleteCategory(int id)
    {
       // 1. Kategoriyi ve içindeki kaynakları bul
        var category = await _context.Categories
                                     .Include(c => c.Resources) // İlişkili kaynakları da getir
                                     .FirstOrDefaultAsync(c => c.Id == id);
        // 2. Kategori yoksa hata ver
        if (category == null)
        {
            return NotFound("Böyle bir kategori bulunamadı.");
        }

        // 3. KUTU DOLU MU? (Güvenlik Önlemi)
        if (category.Resources.Any())
        {
            return BadRequest($"Bu kategoriyi silemezsin! İçinde {category.Resources.Count} adet kaynak var. Önce onları silmelisin.");
        }

        // 4. İçi boşsa sil
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok($"'{category.Name}' kategorisi başarıyla silindi.");
    }}