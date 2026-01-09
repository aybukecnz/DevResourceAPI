using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch; // <-- BU SATIR ÇOK ÖNEMLİ
using Microsoft.AspNetCore.Mvc.ModelBinding; // ModelState için

namespace DevResourceAPI.Services;

public interface IResourceService
{
    Task<object> GetAllResourcesAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<Resource?> GetResourceByIdAsync(int id);
    Task<Resource> CreateResourceAsync(Resource resource, int userId);
    Task<(bool Success, string Message)> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole);
    Task<(bool Success, string Message)> DeleteResourceAsync(int id, int currentUserId, string currentUserRole);
    
    // Patch Metodu
    Task<(bool Success, string Message)> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole, ModelStateDictionary modelState);
}