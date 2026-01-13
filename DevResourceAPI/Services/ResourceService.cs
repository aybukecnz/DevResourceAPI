using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using System.Drawing;

namespace DevResourceAPI.Services;

public class ResourceService : IResourceService
{
    private readonly AppDbContext _context;

    public ResourceService(AppDbContext context)
    {
        _context = context;
    }

    // --- 1. LİSTELEME ---
    public async Task<(IEnumerable<ResourceDto> Data, int TotalRecords)> GetAllResourcesAsync(
        string? search, int? categoryId, int? userId, int pageNumber, int pageSize, int? currentUserId)
    {
        var query = _context.Resources
            .Include(r => r.Category)
            .Include(r => r.User)
            .Include(r => r.Likes)
            .AsQueryable();

        // ARAMA
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r => (r.Title != null && r.Title.ToLower().Contains(search)) || 
                                     (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        // FİLTRELEME
        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        // Toplam Kayıt Sayısı
        var totalRecords = await query.CountAsync();

        // SIRALAMA (Her durumda geçerli)
        query = query.OrderByDescending(r => r.Id);

        // --- DEĞİŞİKLİK BURADA: SAYFALAMA MANTIĞI ---
        // Eğer pageSize -1 DEĞİLSE sayfalama yap. -1 ise bu bloğu atla (Hepsini çek).
        if (pageSize != -1)
        {
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        // PROJECTION (Veriyi DTO'ya çevirme)
        var result = await query
            .Select(r => new ResourceDto 
            {
                Id = r.Id,
                Title = r.Title ?? "Başlıksız",
                Description = r.Description ?? "",
                Url = r.Url ?? "",
                CategoryId = r.CategoryId,
                CategoryName = r.Category != null ? r.Category.Name : "Kategorisiz",
                OwnerName = r.User != null ? r.User.Username : "Bilinmiyor",
                LikeCount = r.Likes.Count,
                IsLikedByMe = currentUserId.HasValue && r.Likes.Any(l => l.UserId == currentUserId.Value)
            })
            .ToListAsync();

        return (result, totalRecords);
    }

    // Gruplama Metodu
    public async Task<IEnumerable<UserGroupedResourceDto>> GetAllResourcesGroupedAsync()
    {
        var resources = await _context.Resources
            .Include(r => r.Category)
            .Include(r => r.User)
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        // Null User kontrolü yaparak gruplama
        var groupedData = resources
            .Where(r => r.User != null) // User'ı olmayanları (silinmişleri) eledik
            .GroupBy(r => r.User!.Username) // (!) null olmadığını garanti ettik
            .Select(group => new UserGroupedResourceDto
            {
                OwnerName = group.Key,
                Resources = group.Select(r => new ResourceDto
                {
                    Id = r.Id,
                    Title = r.Title ?? "Başlıksız",
                    Description = r.Description ?? "",
                    Url = r.Url ?? "",
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category?.Name ?? "Kategorisiz",
                    OwnerName = group.Key
                }).ToList()
            })
            .ToList();

        return groupedData;
    }

    public async Task<Resource?> GetResourceByIdAsync(int id) => await _context.Resources.FindAsync(id);

    public async Task<Resource> CreateResourceAsync(Resource resource, int userId)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == resource.CategoryId);
        if (category == null) throw new Exception("Kategori bulunamadı.");
        if (category.UserId != userId) throw new Exception("Bu kategori size ait değil.");

        resource.Description ??= ""; // Eğer null geldiyse boş string yap
        resource.UserId = userId;
        
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    public async Task<(bool Success, string Message)> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole)
    {
        var existing = await _context.Resources.FindAsync(id);
        if (existing == null) return (false, "Kaynak bulunamadı.");
        
        if (existing.UserId != currentUserId && currentUserRole != "Manager") 
            return (false, "Yetkisiz işlem.");

        if (resource.CategoryId != existing.CategoryId)
        {
            var targetCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == resource.CategoryId);
            if (targetCategory == null) return (false, "Hedef kategori yok.");
            if (targetCategory.UserId != currentUserId && currentUserRole != "Manager")
                return (false, "Kategori transferi yasak.");
        }

        existing.Title = resource.Title;
        existing.Url = resource.Url;
        existing.Description = resource.Description ?? ""; // Null check
        existing.CategoryId = resource.CategoryId;
        
        await _context.SaveChangesAsync();
        return (true, "Güncellendi.");
    }

    // PATCH METHODU (Hatasız Versiyon)
    public async Task<(bool Success, string Message)> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return (false, "Kaynak bulunamadı.");

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return (false, "Yetkisiz işlem.");

        var oldCategoryId = resource.CategoryId;

        // DİKKAT: ModelState parametresini kaldırdık, direkt objeye uyguluyoruz.
        patchDoc.ApplyTo(resource);

        if (resource.CategoryId != oldCategoryId)
        {
             var targetCategory = await _context.Categories.FindAsync(resource.CategoryId);
             if (targetCategory == null) return (false, "Geçersiz kategori.");
             if (targetCategory.UserId != currentUserId && currentUserRole != "Manager")
                 return (false, "Kategori transferi yasak.");
        }

        await _context.SaveChangesAsync();
        return (true, "Kısmi güncelleme başarılı.");
    }

    public async Task<(bool Success, string Message)> DeleteResourceAsync(int id, int currentUserId, string currentUserRole)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return (false, "Kaynak bulunamadı.");

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return (false, "Yetkisiz.");

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return (true, "Silindi.");
    }
}