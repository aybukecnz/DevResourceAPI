using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using DevResourceAPI.Models.Common; // PagedResult için

namespace DevResourceAPI.Services;

public class ResourceService : IResourceService
{
    private readonly AppDbContext _context;

    public ResourceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<PagedResult<ResourceDto>>> GetResourcesAsync(
        int? categoryId, 
        string? search, 
        int pageNumber, 
        int pageSize)
    {
        var query = _context.Resources
            .Include(r => r.Category) 
            .Include(r => r.User)    
            .AsNoTracking()
            .AsQueryable();

        // 1. Filtrelemeler
        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(search) || 
                                     r.Description!.ToLower().Contains(search));
        }

        // 2. Toplam Kayıt Sayısı
        var totalRecords = await query.CountAsync();

        // 3. Sıralama (En yeniden en eskiye)
        query = query.OrderByDescending(r => r.CreatedAt);

        // 4. Sayfalama
        if (pageSize > 0)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        // 5. Veriyi Çekme
        var resources = await query
            .Select(r => new ResourceDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description!,
                Url = r.Url,
                
                // ARTIK 0 GELMEYECEK, FRONTEND İÇİN ÖNEMLİ
                CategoryId = r.CategoryId, 
                
                CategoryName = r.Category != null ? r.Category.Name : "Genel",
                CreatedBy = r.User != null ? r.User.UserName! : "Anonim",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var pagedResult = new PagedResult<ResourceDto>(
            resources,      
            totalRecords,   
            pageNumber,     
            pageSize        
        );

        return ServiceResult<PagedResult<ResourceDto>>.Ok(pagedResult);
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
        
        if (category.UserId != userId) return ServiceResult<Resource>.Fail("Bu kategori size ait değil.");

        resource.Description ??= "";
        resource.UserId = userId;
        resource.CreatedAt = DateTime.UtcNow;
        
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

        //  SOFT DELETE UYGULANDI
        // _context.Resources.Remove(resource); // ESKİ (Hard Delete)
        
        resource.IsDeleted = true; // YENİ (Soft Delete)
        resource.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Kaynak silindi.");
    }
}