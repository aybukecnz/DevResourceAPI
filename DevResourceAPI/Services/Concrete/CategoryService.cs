using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.DTOs;
using DevResourceAPI.Models.Common; // ServiceResult ve PagedResult için gerekli olabilir

namespace DevResourceAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    // 1. KATEGORİLERİ LİSTELEME (SAYFALAMA + ARAMA)
    public async Task<ServiceResult<PagedResult<CategoryDto>>> GetCategoryAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _context.Categories
            .Include(c => c.User)
            .AsNoTracking() // Sadece okuma yaptığımız için performansı artırır
            .AsQueryable();

        // A. Arama Filtresi
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search));
        }

        // B. Toplam Kayıt Sayısı (Sayfalama hesabı için şart)
        var totalRecords = await query.CountAsync();
        
        // C. Sıralama 
        query = query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);

        // D. Sayfalama (Veriyi burada bölüyoruz)
        if (pageSize > 0)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        // E. Veriyi Veritabanından Çekme (Somutlaştırma)
        var list = await query
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                OwnerName = c.User != null ? c.User.UserName! : "Sistem"
            })
            .ToListAsync(); // Veriler burada RAM'e gelir

        // F. Sonuç Paketleme
        var pagedResult = new PagedResult<CategoryDto>(
            list,           // Sayfadaki veriler
            totalRecords,   // Toplam veri sayısı
            pageNumber,     // Şu anki sayfa
            pageSize        // Sayfa boyutu
        );

        return ServiceResult<PagedResult<CategoryDto>>.Ok(pagedResult);
    }

    // 2. TEK KATEGORİ GETİRME
    public async Task<ServiceResult<Category?>> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        
        if (category == null) 
            return ServiceResult<Category?>.Fail("Kategori bulunamadı.");
            
        return ServiceResult<Category?>.Ok(category);
    }

    // 3. KATEGORİ OLUŞTURMA
    public async Task<ServiceResult<Category>> CreateCategoryAsync(Category category, int userId)
    {
        // Aynı isimde kategori var mı?
        bool exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
        if (exists) 
            return ServiceResult<Category>.Fail("Bu isimde bir kategori zaten var.");

        category.UserId = userId;
        category.CreatedAt = DateTime.UtcNow;
        // IsDeleted varsayılan olarak false gelir, elle yazmaya gerek yok.
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return ServiceResult<Category>.Ok(category, "Kategori başarıyla oluşturuldu.");
    }

    // 4. KATEGORİ GÜNCELLEME
    public async Task<ServiceResult> UpdateCategoryAsync(int id, Category category, int currentUserId, string currentUserRole)
    {
        var existing = await _context.Categories.FindAsync(id);
        if (existing == null) 
            return ServiceResult.Fail("Kategori bulunamadı.");

        // Yetki Kontrolü
        if (existing.UserId != currentUserId && currentUserRole != "Manager")
            return ServiceResult.Fail("Bu işlem için yetkiniz yok.");

        existing.Name = category.Name;
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Kategori güncellendi.");
    }

    // 5. KATEGORİ SİLME (SOFT DELETE)
    public async Task<ServiceResult> DeleteCategoryAsync(int id, int currentUserId, string currentUserRole)
    {
        // İlişkili kaynakları (Resources) kontrol etmek için Include yapıyoruz
        var category = await _context.Categories
            .Include(c => c.Resources)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) 
            return ServiceResult.Fail("Kategori bulunamadı.");

        // Yetki Kontrolü
        if (category.UserId != currentUserId && currentUserRole != "Manager")
            return ServiceResult.Fail("Bu işlem için yetkiniz yok.");

        // İş Kuralı: İçi dolu kategori silinemez
        if (category.Resources.Any()) 
            return ServiceResult.Fail("Kategori dolu! Önce içindeki kaynakları silmelisiniz.");

        // SOFT DELETE İŞLEMİ BURADA 
        category.IsDeleted = true;            // Silindi bayrağını kaldır
        category.UpdatedAt = DateTime.UtcNow; // Silinme tarihini güncelle 

        _context.Categories.Update(category); 
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Kategori başarıyla silindi (Geri Dönüşüm Kutusuna Atıldı).");
    }
}