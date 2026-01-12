using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch; // Patch için şart
using Microsoft.AspNetCore.Mvc.ModelBinding; // ModelState için
using Microsoft.AspNetCore.Mvc;

namespace DevResourceAPI.Services;

public class ResourceService : IResourceService
{
    private readonly AppDbContext _context;

    public ResourceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetAllResourcesAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var query = _context.Resources.Include(r => r.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
    {
    // PostgreSQL için optimize edilmiş arama (Büyük/Küçük harf hassasiyeti olmadan)
    query = query.Where(r => 
        EF.Functions.ILike(r.Title, $"%{searchTerm}%") || 
        (r.Category != null && EF.Functions.ILike(r.Category.Name, $"%{searchTerm}%"))
    );
    }

        var totalRecords = await query.CountAsync();
        var result = await query
            .OrderBy(r => r.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ResourceDto 
            {
                Id = r.Id,
                Title = r.Title,
                Url = r.Url,
                CategoryName = r.Category != null ? r.Category.Name : "Tanımsız",
                OwnerName = r.User != null ? r.User.Username : "Silinmiş Kullanıcı"
            })
            .ToListAsync();

        return new { TotalRecords = totalRecords, Data = result };
    }

    public async Task<Resource?> GetResourceByIdAsync(int id) => await _context.Resources.FindAsync(id);

    public async Task<Resource> CreateResourceAsync(Resource resource, int userId)
{
    // Kategoriyi veritabanından çek (Sadece var mı diye değil, kimin diye bakmak için)
    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == resource.CategoryId);

    // Kategori hiç yoksa hata ver
    if (category == null)
    {
        throw new Exception("Böyle bir kategori bulunamadı.");
    }

    // GÜVENLİK DUVARI: Kategori senin mi? 
    // Eğer kategorinin sahibi (category.UserId), şu anki işlem yapan kişi (userId) değilse DUR!
    if (category.UserId != userId)
    {
        throw new Exception("Hata: Başkasının kategorisine kaynak ekleyemezsiniz!");
    }

    resource.UserId = userId;
    _context.Resources.Add(resource);
    await _context.SaveChangesAsync();
    return resource;
}

    public async Task<(bool Success, string Message)> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole)
{
    // Mevcut kaynağı çek (Tracking açık olsun, güncelleyeceğiz)
    var existing = await _context.Resources.FindAsync(id);
    if (existing == null) return (false, "Kaynak bulunamadı.");
    
    // Kaynağın sahibi sen misin?
    if (existing.UserId != currentUserId && currentUserRole != "Manager") 
        return (false, "Yetkisiz işlem.");

    // --- YENİ EKLENEN GÜVENLİK KONTROLÜ ---
    // Eğer kullanıcı kategoriyi değiştirmek istiyorsa, o yeni kategori onun mu?
    if (resource.CategoryId != existing.CategoryId)
    {
        var targetCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == resource.CategoryId);
        if (targetCategory == null) return (false, "Hedef kategori bulunamadı.");
        
        // Manager değilse ve Kategori başkasınınsa DUR!
        if (targetCategory.UserId != currentUserId && currentUserRole != "Manager")
            return (false, "Başkasının kategorisine kaynak taşıyamazsınız!");
    }
    // Alanları güncelle
    existing.Title = resource.Title;
    existing.Url = resource.Url;
    existing.CategoryId = resource.CategoryId; // Güvenli bir şekilde güncellendi
    // existing.UserId = ... (Buna dokunma, sahiplik değişmez)
    
    await _context.SaveChangesAsync();
    return (true, "Güncellendi.");
}

    public async Task<(bool Success, string Message)> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole, ModelStateDictionary modelState)
{
    var resource = await _context.Resources.FindAsync(id);
    if (resource == null) return (false, "Kaynak bulunamadı.");

    // Kaynağın sahibi kontrolü
    if (resource.UserId != currentUserId && currentUserRole != "Manager")
        return (false, "Yetkisiz işlem.");

    // Eski CategoryId'yi hafızada tut
    var oldCategoryId = resource.CategoryId;

    // Değişiklikleri uygula (Henüz DB'ye gitmedi, hafızada)
    patchDoc.ApplyTo(resource, modelState);

    //  YENİ EKLENEN KONTROL 
    // Eğer Patch işlemi kategoriyi değiştirdiyse?
    if (resource.CategoryId != oldCategoryId)
    {
         var targetCategory = await _context.Categories.FindAsync(resource.CategoryId);
         if (targetCategory == null) return (false, "Hedef kategori geçersiz.");

         // Manager değilse ve kategori başkasınınsa?
         if (targetCategory.UserId != currentUserId && currentUserRole != "Manager")
             return (false, "Başkasının kategorisine transfer yapamazsınız!");
    }

    await _context.SaveChangesAsync();
    return (true, "Kısmi güncelleme başarılı.");
}
    public async Task<(bool Success, string Message)> DeleteResourceAsync(int id, int currentUserId, string currentUserRole)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return (false, "Kaynak bulunamadı.");

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return (false, "Yetkisiz işlem.");

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return (true, "Silindi.");
    }
}