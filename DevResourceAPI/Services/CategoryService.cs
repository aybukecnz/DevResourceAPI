using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        public CategoryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c=> c.User) 
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    OwnerName = c.User !=null ? c.User.Username : "Anonim"
                })
                .ToListAsync();
        }
    public async Task<Category> CreateCategoryAsync(Category category, int userId)
        {
            category.UserId = userId;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id, int currentUserId, string currentUserRole)
        {
            var category = await _context.Categories
                .Include(c => c.Resources)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return (false, "Kategori bulunamadı.");

            // YETKİ KONTROLÜ (Open Source Mantığı)
            if (category.UserId != currentUserId && currentUserRole != "Manager")
                return (false, "Bu işlem için yetkiniz yok.");

            if (category.Resources.Any())
                return (false, "Kategori dolu! Önce içindeki kaynakları silin.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return (true, "Başarıyla silindi.");
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }
    }
}    
    

