using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using DevResourceAPI.Services;

namespace DevResourceAPI.Services;

public class ResourceService : IResourceService
{
    private readonly AppDbContext _context;

    public ResourceService(AppDbContext context)
    {
        _context = context;
    }

    // ENTERPRISE IMPL: PagedResult kullanılıyor
    public async Task<ServiceResult<PagedResult<ResourceDto>>> GetAllResourcesAsync(
        string? search, 
        int? categoryId, 
        int? userId, 
        int pageNumber, 
        int pageSize, 
        int? currentUserId)
    {
        var query = _context.Resources
            .Include(r => r.Category)
            .Include(r => r.User)
            .Include(r => r.Likes)
            .AsQueryable();

        // --- ARAMA & FİLTRELEME ---
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r => (r.Title != null && r.Title.ToLower().Contains(search)) || 
                                     (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        if (categoryId.HasValue)
            query = query.Where(r => r.CategoryId == categoryId.Value);

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        // --- SAYFALAMA MANTIĞI ---
        var totalRecords = await query.CountAsync();
        query = query.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt);

        if (pageSize > 0)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        // --- DTO ÇEVİRİMİ ---
        var list = await query
            .Select(r => new ResourceDto 
            {
                Id = r.Id,
                Title = r.Title ?? "Başlıksız",
                Description = r.Description ?? "",
                Url = r.Url ?? "",
                CategoryId = r.CategoryId,
                CategoryName = r.Category != null ? r.Category.Name : "Kategorisiz",
                OwnerName = r.User != null ? r.User.UserName! : "Bilinmiyor",
                LikeCount = r.Likes.Count,
                IsLikedByMe = currentUserId.HasValue && r.Likes.Any(l => l.UserId == currentUserId.Value)
            })
            .ToListAsync();

        // PAKETLEME (Enterprise Dokunuşu)
        var pagedData = new PagedResult<ResourceDto>(list, totalRecords);

        return ServiceResult<PagedResult<ResourceDto>>.Ok(pagedData);
    }

    public async Task<ServiceResult<Resource?>> GetResourceByIdAsync(int id)
    {
        var resource = await _context.Resources
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (resource == null)
            return ServiceResult<Resource?>.Fail("Kaynak bulunamadı."); 
            
        return ServiceResult<Resource?>.Ok(resource);
    }

    public async Task<ServiceResult<Resource>> CreateResourceAsync(Resource resource, int userId)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == resource.CategoryId);
        if (category == null) return ServiceResult<Resource>.Fail("Kategori bulunamadı.");
        
        // Kategori başkasınınsa ekleme yapamaz (Opsiyonel kural, istersen kaldırabilirsin)
        if (category.UserId != userId) return ServiceResult<Resource>.Fail("Bu kategori size ait değil.");

        resource.Description ??= "";
        resource.UserId = userId;
        resource.CreatedAt = DateTime.UtcNow; // Tarih ataması önemli
        
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return ServiceResult<Resource>.Ok(resource);
    }

    public async Task<ServiceResult> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole)
    {
        var existing = await _context.Resources.FindAsync(id);
        if (existing == null) return ServiceResult.Fail("Kaynak bulunamadı.");
        
        if (existing.UserId != currentUserId && currentUserRole != "Manager") 
            return ServiceResult.Fail("Yetkisiz işlem.");

        if (resource.CategoryId != existing.CategoryId)
        {
            var targetCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == resource.CategoryId);
            if (targetCategory == null) return ServiceResult.Fail("Hedef kategori yok.");
            
            if (targetCategory.UserId != currentUserId && currentUserRole != "Manager")
                return ServiceResult.Fail("Kategori transferi yasak.");
        }

        existing.Title = resource.Title;
        existing.Url = resource.Url;
        existing.Description = resource.Description ?? "";
        existing.CategoryId = resource.CategoryId;
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Güncellendi.");
    }

    public async Task<ServiceResult> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return ServiceResult.Fail("Kaynak bulunamadı.");

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return ServiceResult.Fail("Yetkisiz işlem.");

        var oldCategoryId = resource.CategoryId;

        patchDoc.ApplyTo(resource);

        if (resource.CategoryId != oldCategoryId)
        {
             var targetCategory = await _context.Categories.FindAsync(resource.CategoryId);
             if (targetCategory == null) return ServiceResult.Fail("Geçersiz kategori.");
             
             if (targetCategory.UserId != currentUserId && currentUserRole != "Manager")
                 return ServiceResult.Fail("Kategori transferi yasak.");
        }

        resource.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Kısmi güncelleme başarılı.");
    }

    public async Task<ServiceResult> DeleteResourceAsync(int id, int currentUserId, string currentUserRole)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return ServiceResult.Fail("Kaynak bulunamadı.");

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return ServiceResult.Fail("Yetkiniz yok.");

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Kaynak silindi.");
    }
}