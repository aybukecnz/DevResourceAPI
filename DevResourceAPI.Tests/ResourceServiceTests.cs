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
    [Fact]
    public async Task CreateResource_ShouldAddResource_WhenValidRequest()
    {
        // 1. ARRANGE (Hazırlık)
        var context = GetInMemoryDbContext();
        var service = new ResourceService(context);

        // Önce gerekli yan verileri (User & Category) ekleyelim
        var user = new User { UserName = "aybuke", Email = "test@test.com", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var category = new Category { Name = "Backend", UserId = user.Id, CreatedAt = DateTime.UtcNow };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        // Eklenecek yeni kaynağı hazırlayalım
        var newResource = new Resource
        {
            Title = "GitHub Actions",
            Url = "https://github.com",
            Description = "CI/CD Öğreniyorum",
            CategoryId = category.Id
        };

        // 2. ACT (Eylem - Metodu Çalıştır)
        // Not: CreateResourceAsync metodun sadece Resource nesnesi ve UserId alıyor olmalı.
        var result = await service.CreateResourceAsync(newResource, user.Id);

        // 3. ASSERT (Doğrulama)
        Assert.True(result.Success, "Ekleme başarısız: " + result.Message);
        Assert.True(result.Data!.Id > 0, "Yeni eklenen kaynağın ID'si 0 olmamalı.");
        
        // Veritabanını kontrol et
        var dbData = await context.Resources.FirstOrDefaultAsync(r => r.Title == "GitHub Actions");
        Assert.NotNull(dbData); // Veritabanında kayıt oluşmuş mu?
    }
    [Fact]
    public async Task DeleteResource_ShouldReturnSuccess_WhenResourceExists()
    {
        // 1. ARRANGE (Hazırlık)
        var context = GetInMemoryDbContext();
        var service = new ResourceService(context);

        // Önce silinecek veriyi oluşturalım
        var user = new User { UserName = "silici_kullanici", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var resource = new Resource 
        { 
            Title = "Silinecek Kaynak", 
            Url = "https://delete.me", 
            UserId = user.Id,
            CategoryId = 1,
            IsDeleted = false
        };
        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        // 2. ACT (Eylem)
        // Kullanıcı kendi oluşturduğu kaynağı siliyor
        var result = await service.DeleteResourceAsync(resource.Id, user.Id, currentUserRole: "User");

        // 3. ASSERT (Doğrulama)
        Assert.True(result.Success, "Silme işlemi başarısız oldu: " + result.Message);

        // Veritabanından kontrol edelim
        var deletedResource = await context.Resources.FindAsync(resource.Id);
        
        Assert.NotNull(deletedResource); // Veri fiziksel olarak hala orada (null değil)
        Assert.True(deletedResource.IsDeleted, "Hata: IsDeleted alanı true olarak işaretlenmedi!"); // Ama silindi olarak işaretli
    }
        [Fact]
public async Task DeleteResource_ShouldReturnError_WhenUserIsNotOwner()
{
    // 1. ARRANGE
    var context = GetInMemoryDbContext();
    var service = new ResourceService(context);

    // Bir asıl sahip (Owner), bir de kötü niyetli (Stranger) kullanıcı oluşturalım
    var owner = new User { UserName = "owner", CreatedAt = DateTime.UtcNow };
    var stranger = new User { UserName = "stranger", CreatedAt = DateTime.UtcNow };
    context.Users.AddRange(owner, stranger);
    await context.SaveChangesAsync();

    // Owner'a ait bir kaynak ekleyelim
    var resource = new Resource { Title = "Gizli Kaynak", UserId = owner.Id, CategoryId = 1 };
    context.Resources.Add(resource);
    await context.SaveChangesAsync();

    // 2. ACT
    // Dikkat: Kaynak Owner'ın, ama silmeye çalışan Stranger!
    var result = await service.DeleteResourceAsync(resource.Id, stranger.Id, "User");

    // 3. ASSERT
    Assert.False(result.Success); // Başarısız olmalı
    Assert.Contains("yetkiniz yok", result.Message.ToLower()); // Uyarı mesajı vermeli
    
    // Veritabanında hala silinmemiş olmalı
    var dbResource = await context.Resources.FindAsync(resource.Id);
    Assert.False(dbResource!.IsDeleted); 
}
}