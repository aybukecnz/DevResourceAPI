using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch;

namespace DevResourceAPI.Services;

public interface IResourceService
{
    // ENTERPRISE DÖNÜŞÜMÜ: Tuple yerine PagedResult
    Task<ServiceResult<PagedResult<ResourceDto>>> GetAllResourcesAsync(
        string? search,
        int? categoryId, 
        int? userId,
        int pageNumber, 
        int pageSize, 
        int? currentUserId);

    Task<ServiceResult<Resource?>> GetResourceByIdAsync(int id);
    
    Task<ServiceResult<Resource>> CreateResourceAsync(Resource resource, int userId);
    
    Task<ServiceResult> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole);
    
    Task<ServiceResult> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole);
    
    Task<ServiceResult> DeleteResourceAsync(int id, int currentUserId, string currentUserRole);
}