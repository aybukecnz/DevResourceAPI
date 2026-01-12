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
        // Kategori var mı ?
    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == resource.CategoryId);
    if (!categoryExists)
    {
        throw new Exception("Geçersiz Kategori ID'si."); // Veya özel bir hata fırlat
    }
        resource.UserId = userId;
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    public async Task<(bool Success, string Message)> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole)
    {
        var existing = await _context.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (existing == null) return (false, "Kaynak bulunamadı.");
        
        if (existing.UserId != currentUserId && currentUserRole != "Manager") 
            return (false, "Yetkisiz işlem.");

        resource.Id = id; 
        resource.UserId = existing.UserId; 
        _context.Entry(resource).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
        return (true, "Güncellendi.");
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

    public async Task<(bool Success, string Message)> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole, ModelStateDictionary modelState)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return (false, "Kaynak bulunamadı.");

        if (resource.UserId != currentUserId && currentUserRole != "Manager")
            return (false, "Yetkisiz işlem.");

        patchDoc.ApplyTo(resource, modelState);
        await _context.SaveChangesAsync();
        return (true, "Kısmi güncelleme başarılı.");
    }
}