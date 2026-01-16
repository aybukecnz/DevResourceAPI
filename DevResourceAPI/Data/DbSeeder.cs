using DevResourceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevResourceAPI.Data;

public static class DbSeeder 
{
    // Parametre olarak 'configuration' alıyoruz ki şifreleri okuyabilelim
    public static async Task SeedData(IApplicationBuilder app, IConfiguration configuration)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Veritabanı yoksa oluştur
            context.Database.EnsureCreated();

            // Eğer içeride zaten kullanıcı varsa tekrar ekleme yapma, çık.
            if (await userManager.Users.AnyAsync()) return;

            // --- AYARLARI OKU ---
            // appsettings.json'dan okur. Bulamazsa "Default" değerleri kullanır.
            var adminPassword = configuration["SeedSettings:AdminPassword"] ?? "Password123!";
        //  var adminEmail = configuration["SeedSettings:AdminEmail"] ?? "admin@localhost";

            // 1. ROLLERİ OLUŞTUR
            var roles = new[] { "Manager", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            // 2. MANAGER KULLANCISI 
            var adminUser = new User
            {
                UserName = "aybuke", 
            //    Email = adminEmail,
                Role = "Manager",
                CreatedAt = DateTime.UtcNow,
            //    EmailConfirmed = true
            };

            // Şifreyi ayarlardan gelen değerle oluştur
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Manager"); 
            }

            // 3. NORMAL KULLANICI - Test için
            var normalUser = new User
            {
                UserName = "ahmet",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
            //    EmailConfirmed = true
            };
            
            var res2 = await userManager.CreateAsync(normalUser, "Password123!"); 
            if (res2.Succeeded)
            {
                await userManager.AddToRoleAsync(normalUser, "User");
            }

            // 4. ÖRNEK VERİLER (Kategori ve Kaynak)
            // Aybüke oluştuysa onun adına veri ekleyelim
            var aybukeDb = await userManager.FindByNameAsync("aybuke");

            if (aybukeDb != null)
            {
                var category = new Category
                {
                    Name = "Yazılım",
                    UserId = aybukeDb.Id,
                    CreatedAt = DateTime.UtcNow
                };

                context.Categories.Add(category);
                await context.SaveChangesAsync(); 

                var resource = new Resource
                {
                    Title = "ASP.NET Core Docs",
                    Description = "Microsoft'un resmi dokümantasyonu",
                    Url = "https://learn.microsoft.com/aspnet/core",
                    CategoryId = category.Id,
                    UserId = aybukeDb.Id,
                    CreatedAt = DateTime.UtcNow
                };

                context.Resources.Add(resource);
                await context.SaveChangesAsync();
            }
        }
    }
}