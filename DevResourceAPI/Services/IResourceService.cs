using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch;

namespace DevResourceAPI.Services;

public interface IResourceService
{
    Task<(IEnumerable<ResourceDto> Data, int TotalRecords)> GetAllResourcesAsync(string? search, int? categoryId, int? userId,int pageNumber, int pageSize, int? currentUserId);
    Task<IEnumerable<UserGroupedResourceDto>> GetAllResourcesGroupedAsync();
    
    Task<Resource?> GetResourceByIdAsync(int id);
    Task<Resource> CreateResourceAsync(Resource resource, int userId);
    Task<(bool Success, string Message)> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole);
    
    // DÜZELTME: Sondaki 'ModelStateDictionary modelState' kısmını SİLDİK
    Task<(bool Success, string Message)> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole);
    
    Task<(bool Success, string Message)> DeleteResourceAsync(int id, int currentUserId, string currentUserRole);
}