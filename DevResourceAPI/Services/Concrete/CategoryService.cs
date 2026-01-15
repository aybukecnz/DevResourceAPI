using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<PagedResult<CategoryDto>>> GetAllCategoriesAsync(
        string? search, 
        int pageNumber, 
        int pageSize)
    {
        var query = _context.Categories
            .Include(c => c.User)
            .AsQueryable();

        // 1. Arama
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c => c.Name != null && c.Name.ToLower().Contains(search));
        }

        // 2. Toplam Sayı
        var totalRecords = await query.CountAsync();
        
        // 3. Sıralama ve Sayfalama
        query = query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);

        if (pageSize > 0)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        // 4. Veriyi Çekme
        var list = await query
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                OwnerName = c.User != null ? c.User.UserName! : "Sistem"
            })
            .ToListAsync();

        // 5. PAKETLEME (Enterprise Dokunuşu)
        var pagedData = new PagedResult<CategoryDto>(list, totalRecords);

        return ServiceResult<PagedResult<CategoryDto>>.Ok(pagedData);
    }

    public async Task<ServiceResult<Category?>> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return ServiceResult<Category?>.Fail("Kategori bulunamadı.");
        return ServiceResult<Category?>.Ok(category);
    }

    public async Task<ServiceResult<Category>> CreateCategoryAsync(Category category, int userId)
    {
        bool exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
        if (exists) return ServiceResult<Category>.Fail("Bu isimde bir kategori zaten var.");

        category.UserId = userId;
        category.CreatedAt = DateTime.UtcNow;
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return ServiceResult<Category>.Ok(category, "Kategori oluşturuldu.");
    }

    public async Task<ServiceResult> UpdateCategoryAsync(int id, Category category, int currentUserId, string currentUserRole)
    {
        var existing = await _context.Categories.FindAsync(id);
        if (existing == null) return ServiceResult.Fail("Kategori bulunamadı.");

        if (existing.UserId != currentUserId && currentUserRole != "Manager")
            return ServiceResult.Fail("Yetkiniz yok.");

        existing.Name = category.Name;
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Kategori güncellendi.");
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id, int currentUserId, string currentUserRole)
    {
        var category = await _context.Categories.Include(c => c.Resources).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return ServiceResult.Fail("Kategori bulunamadı.");

        if (category.UserId != currentUserId && currentUserRole != "Manager")
            return ServiceResult.Fail("Yetkiniz yok.");

        // GÜVENLİK: Dolu kategori silinemez
        if (category.Resources.Any()) 
            return ServiceResult.Fail("Kategori dolu! Önce içindeki kaynakları silin.");

       category.IsDeleted = true;            // Silindi olarak işaretle
    category.UpdatedAt = DateTime.UtcNow; // Ne zaman silindiğini güncelle

    // Remove yerine Update kullanıyoruz (veya hiç bir şey yazmasan da EF Core değişikliği algılar)
    _context.Categories.Update(category); 
    
    await _context.SaveChangesAsync();

    return ServiceResult.Ok("Kategori başarıyla silindi (Soft Delete).");
    }
}