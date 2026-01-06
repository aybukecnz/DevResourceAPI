using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Data;
using DevResourceAPI.Models;

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
}