using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.DTOs;
using DevResourceAPI.Models.Common;

namespace DevResourceAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        public CategoryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<(IEnumerable<CategoryDto> Data, int TotalRecords)> GetAllCategoriesAsync(
        string? search, 
        int pageNumber, 
        int pageSize)
    {
        var query = _context.Categories
            .Include(c => c.User) // Kategoriyi kimin oluşturduğunu görmek istersen
            .AsQueryable();

        // 1. ARAMA (Kategori ismine göre)
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c => c.Name != null && c.Name.ToLower().Contains(search));
        }

        // 2. SAYFALAMA MANTIĞI
        var totalRecords = await query.CountAsync();
        query = query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt); // Yeniden eskiye

        // pageSize > 0 ise sayfalama yap (Resource ile aynı mantık)
        if (pageSize > 0)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        // 3. DTO ÇEVİRİMİ
        var result = await query
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                // Eğer CategoryDto'da UserId veya OwnerName yoksa bu satırları silebilirsin:
                // OwnerName = c.User != null ? c.User.Username : "Sistem"
            })
            .ToListAsync();

        return (result, totalRecords);
    }
    public async Task<Category> CreateCategoryAsync(Category category, int userId)
        {
            category.UserId = userId;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(int id, Category category, int currentUserId, string currentUserRole)
{
    var existing = await _context.Categories.FindAsync(id);
    if (existing == null) return (false, "Kategori bulunamadı.");

    // Yetki Kontrolü: Sahibi mi? Veya Manager mı?
    if (existing.UserId != currentUserId && currentUserRole != "Manager")
        return (false, "Bu işlemi yapmaya yetkiniz yok.");

    // Sadece ismini güncelle, UserId veya Id'ye dokunma!
    existing.Name = category.Name;
    
    await _context.SaveChangesAsync();
    return (true, "Kategori güncellendi.");
}
        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id, int currentUserId, string currentUserRole)
        {
            var category = await _context.Categories
                .Include(c => c.Resources)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return (false, "Kategori bulunamadı.");

            // YETKİ KONTROLÜ (Open Source Mantığı)
            if (category.UserId != currentUserId && currentUserRole != "Manager")
                return (false, "Bu işlem için yetkiniz yok.");

            if (category.Resources.Any())
                return (false, "Kategori dolu! Önce içindeki kaynakları silin.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return (true, "Başarıyla silindi.");
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }
        public async Task<IEnumerable<Category>> GetAllAsync(PaginationFilter filter)
{
    // Eğer tümünü görmek istiyorsa (Örn: PageSize -1 geldiyse)
    // Veya basit bir kontrolle
    var query = _context.Categories.AsQueryable(); // IQueryable ile başlıyoruz (Henüz DB'ye gitmedik)

    // Sayfalama Formülü
    // Skip: Kaç tane kayıt pas geçilecek?
    // Take: Kaç tane kayıt alınacak?
    var pagedData = await query
        .Skip((filter.PageNumber - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ToListAsync(); // Şimdi DB'ye gittik

    return pagedData;
}
    }
}    
    

