using DevResourceAPI.Models;
using DevResourceAPI.DTOs;
using DevResourceAPI.Models.Common;

namespace DevResourceAPI.Services
{
    public interface ICategoryService
    {
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category, int userId);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id, int currentUserId, string currentUserRole);
        Task<(bool Success, string Message)> UpdateCategoryAsync(int id, Category category, int currentUserId, string currentUserRole);
        // Dönüş tipi artık sadece liste değil, (Liste, ToplamSayı) şeklindeki Tuple oldu.
        Task<(IEnumerable<CategoryDto> Data, int TotalRecords)> GetAllCategoriesAsync(string? search, int pageNumber, int pageSize);
    }
}