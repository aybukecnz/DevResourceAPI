using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using Microsoft.AspNetCore.JsonPatch;

namespace DevResourceAPI.Services;

public interface IResourceService
{
    Task<ServiceResult<Resource?>> GetResourceByIdAsync(int id);
    
    Task<ServiceResult<Resource>> CreateResourceAsync(Resource resource, int userId);
    
    Task<ServiceResult> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole);
    
    Task<ServiceResult> PatchResourceAsync(int id, JsonPatchDocument<Resource> patchDoc, int currentUserId, string currentUserRole);
    
    Task<ServiceResult> DeleteResourceAsync(int id, int currentUserId, string currentUserRole);
    Task<ServiceResult<PagedResult<ResourceDto>>> GetResourcesAsync(int? categoryId, string? search, int pageNumber, int pageSize);
}