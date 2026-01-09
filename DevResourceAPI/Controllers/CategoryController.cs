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

    // Constructor: Veritabanı bağlantısını sisteme enjekte et
    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/category 
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
        .Include(c => c.User) 
        .Select(c => new { 
            c.Id, 
            c.Name, 
            OwnerName = c.User != null ? c.User.Username : "Anonim" 
        })
        .ToListAsync();
    return Ok(categories);
    }

    // POST: api/category 
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        // 1. Token'dan Kullanıcı ID'sini Al
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim!= null)
        {
            category.UserId = int.Parse(userIdClaim.Value);
        }
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(category);
    }
    // DELETE: api/category/{id} 
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(int id, [FromQuery] bool confirm = false)
{
    // 1. Onay Kontrolü
    if (!confirm)
    {
        return BadRequest(new { message = "Bu kategoriyi silmek istediğinize emin misiniz? Lütfen 'confirm=true' parametresini ekleyin." });
    }

    // 2. Token'dan Kullanıcı Bilgilerini Al (ID ve Rol)
    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    var userRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role); // Rol bilgisini çekiyoruz

    if (userIdClaim == null || userRoleClaim == null) 
    {
        return Unauthorized(new { message = "Kimlik bilgileri doğrulanamadı. Lütfen tekrar giriş yapın." });
    }

    int currentUserId = int.Parse(userIdClaim.Value);
    string currentUserRole = userRoleClaim.Value;

    // 3. Kategoriyi ve Kaynaklarını Veritabanından Getir
    var category = await _context.Categories
        .Include(c => c.Resources)
        .FirstOrDefaultAsync(c => c.Id == id);

    // 4. Var mı Kontrolü
    if (category == null) 
    {
        return NotFound(new { message = "Kategori bulunamadı." });
    }

    // 5. YETKİ VE SAHİPLİK KONTROLÜ (Hibrit Model)
    // Eğer kullanıcı sahibi DEĞİLSE VE Manager DEĞİLSE silme işlemini reddet
    if (category.UserId != currentUserId && currentUserRole != "Manager")
    {
        return StatusCode(403, new { message = "Bu işlem için yetkiniz yok. Sadece içerik sahibi veya bir yönetici silebilir." });
    }

    // 6. Bağımlılık Kontrolü (İçinde Resource varsa silmeyi engeller)
    if (category.Resources.Any())
    {
        return BadRequest(new { 
            message = $"Kategori boş değil! İçinde {category.Resources.Count} adet kaynak var. Lütfen önce kaynakları silin." 
        });
    }

    // 7. Silme ve Kaydetme
    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Kategori başarıyla silindi." });}
}