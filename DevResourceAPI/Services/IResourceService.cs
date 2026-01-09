using DevResourceAPI.Models;
using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public interface IResourceService
{
    Task<object> GetAllResourcesAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<Resource?> GetResourceByIdAsync(int id);
    Task<Resource> CreateResourceAsync(Resource resource, int userId);
    Task<(bool Success, string Message)> UpdateResourceAsync(int id, Resource resource, int currentUserId, string currentUserRole);
    Task<(bool Success, string Message)> DeleteResourceAsync(int id, int currentUserId, string currentUserRole);
}