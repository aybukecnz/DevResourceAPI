using DevResourceAPI.Data;
using DevResourceAPI.Models;
using DevResourceAPI.DTOs; // DTO kullanımı için
using DevResourceAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevResourceAPI.Tests;

public class ResourceServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllResources_ShouldReturnCorrectPagination_WhenDataExists()
    {
        // 1. ARRANGE (Hazırlık)
        var context = GetInMemoryDbContext();
        
        // Önce ilişkili verileri (User ve Category) oluşturalım
        var user = new User 
        { 
            UserName = "testuser", 
        //    Email = "test@test.com",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync(); // ID oluşsun diye kaydediyoruz

        var category = new Category 
        { 
            Name = "Test Kategori", 
            UserId = user.Id, // User'a bağladık
            CreatedAt = DateTime.UtcNow
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync(); // ID oluşsun diye kaydediyoruz

        // Şimdi bu User ve Category'ye bağlı Resource ekleyelim
        context.Resources.Add(new Resource 
        { 
            Title = "Test Kaynak 1", 
            Url = "https://test1.com", 
            Description = "Açıklama 1", 
            CategoryId = category.Id, // Kategoriye bağladık
            UserId = user.Id,         // User'a bağladık
            CreatedAt = DateTime.UtcNow, 
            IsDeleted = false 
        });

        context.Resources.Add(new Resource 
        { 
            Title = "Test Kaynak 2", 
            Url = "https://test2.com", 
            Description = "Açıklama 2", 
            CategoryId = category.Id, 
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow, 
            IsDeleted = false 
        });
        
        await context.SaveChangesAsync();

        // Servisi başlat
        var service = new ResourceService(context);

        // 2. ACT (Eylem)
        var result = await service.GetResourcesAsync(null, null, 1, 10);

        // 3. ASSERT (Doğrulama)
        Assert.True(result.Success, "İşlem başarısız oldu.");
        Assert.Equal(2, result.Data!.TotalRecords); // 2 kayıt bekliyoruz
        Assert.Equal("Test Kaynak 1", result.Data.Items.OrderBy(r => r.Title).First().Title);
    }
}