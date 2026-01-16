using DevResourceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevResourceAPI.Data;

public static class DbSeeder 
{
    public static async Task SeedData(IApplicationBuilder app, IConfiguration configuration)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Database.EnsureCreated();

            if (await userManager.Users.AnyAsync()) return;

            Console.WriteLine("🌱 Seed Data Başlatılıyor...");

            var roles = new[] { "Manager", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            // 2. MANAGER (Aybüke)
            var adminUser = new User
            {
                UserName = "aybuke", 
            //    Email = "aybuke@devresource.com",
                Role = "Manager",
            //    EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow 
            };

            var adminResult = await userManager.CreateAsync(adminUser, configuration["SeedSettings:AdminPassword"] ?? "Password123!");
            if (adminResult.Succeeded) await userManager.AddToRoleAsync(adminUser, "Manager");

            // 3. MİSAFİR USER
            var guestUser = new User
            {
                UserName = "misafir",
            //    Email = "guest@devresource.com",
                Role = "User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var guestResult = await userManager.CreateAsync(guestUser, "Password123!");
            if (guestResult.Succeeded) await userManager.AddToRoleAsync(guestUser, "User");

            var aybukeDb = await userManager.FindByNameAsync("aybuke");
            if (aybukeDb == null) return;

            // 4. İÇERİK VERİLERİ 
            
            var seedData = new List<(string CategoryName, List<Resource> Resources)>
            {
                ("Backend Development", new List<Resource>
                {
                    new Resource { Title = "ASP.NET Core Roadmap", Description = "Backend geliştiriciler için kapsamlı yol haritası.", Url = "https://roadmap.sh/aspnet-core" },
                    new Resource { Title = "Clean Architecture", Description = "Jason Taylor'ın Clean Architecture prensipleri videosu.", Url = "https://www.youtube.com/watch?v=dK4Yb6-LxAk" },
                    new Resource { Title = "Entity Framework Core Docs", Description = "Microsoft'un resmi EF Core dokümantasyonu.", Url = "https://learn.microsoft.com/en-us/ef/core/" }
                }),
                ("Frontend Development", new List<Resource>
                {
                    new Resource { Title = "React Resmi Dokümanları", Description = "Modern React öğrenmek için en iyi kaynak.", Url = "https://react.dev/" },
                    new Resource { Title = "Tailwind CSS Cheat Sheet", Description = "Hızlı CSS stillendirme için kopya kağıdı.", Url = "https://nerdcave.com/tailwind-cheat-sheet" }
                }),
                ("DevOps & Cloud", new List<Resource>
                {
                    new Resource { Title = "Docker for Beginners", Description = "Docker konteyner mimarisine giriş.", Url = "https://docker-curriculum.com/" },
                    new Resource { Title = "Kubernetes Basics", Description = "K8s temel kavramları ve mimarisi.", Url = "https://kubernetes.io/docs/tutorials/kubernetes-basics/" },
                    new Resource { Title = "Azure Fundamentals", Description = "AZ-900 sertifikası için çalışma notları.", Url = "https://learn.microsoft.com/en-us/credentials/certifications/azure-fundamentals/" }
                }),
                ("Cyber Security", new List<Resource>
                {
                    new Resource { Title = "OWASP Top 10", Description = "Web uygulamaları için en kritik 10 güvenlik riski.", Url = "https://owasp.org/www-project-top-ten/" },
                    new Resource { Title = "Hack The Box", Description = "Sızma testleri için pratik yapma platformu.", Url = "https://www.hackthebox.com/" },
                    new Resource { Title = "Kali Linux Tools", Description = "Siber güvenlik araçları listesi.", Url = "https://www.kali.org/tools/" }
                })
            };

            foreach (var data in seedData)
            {
                var category = new Category
                {
                    Name = data.CategoryName,
                    UserId = aybukeDb.Id
                };

                context.Categories.Add(category);
                await context.SaveChangesAsync(); 

                foreach (var res in data.Resources)
                {
                    res.CategoryId = category.Id;
                    res.UserId = aybukeDb.Id;
                    
                    context.Resources.Add(res);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}