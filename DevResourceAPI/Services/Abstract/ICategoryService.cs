using DevResourceAPI.Models;
using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public interface ICategoryService
{
    Task<ServiceResult<PagedResult<CategoryDto>>> GetCategoryAsync(string? search, int pageNumber, int pageSize);

    Task<ServiceResult<Category?>> GetCategoryByIdAsync(int id);
    Task<ServiceResult<Category>> CreateCategoryAsync(Category category, int userId);
    Task<ServiceResult> UpdateCategoryAsync(int id, Category category, int currentUserId, string currentUserRole);
    Task<ServiceResult> DeleteCategoryAsync(int id, int currentUserId, string currentUserRole);
}