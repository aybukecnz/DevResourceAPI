using DevResourceAPI.Models;

namespace DevResourceAPI.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<object>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category, int userId);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id, int currentUserId, string currentUserRole);
    }
}