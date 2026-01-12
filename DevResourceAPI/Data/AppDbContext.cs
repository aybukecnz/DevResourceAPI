// models ile postresql arası köprü
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Models;

namespace DevResourceAPI.Data;


public class AppDbContext : DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}
    public DbSet<Resource> Resources { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<ResourceLike> ResourceLikes { get; set; }
    public DbSet<UserFollow> UserFollows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // BEĞENİ İLİŞKİSİ 
        
        // Composite Key: Bir kullanıcı aynı kaynağı sadece 1 kere beğenebilir.
        // (UserId + ResourceId) ikilisi benzersiz (Primary Key) olacak.
        modelBuilder.Entity<ResourceLike>()
            .HasKey(rl => new { rl.UserId, rl.ResourceId });

        // İlişki: Bir beğeninin bir kaynağı vardır
        modelBuilder.Entity<ResourceLike>()
            .HasOne(rl => rl.Resource)
            .WithMany(r => r.Likes)
            .HasForeignKey(rl => rl.ResourceId)
            .OnDelete(DeleteBehavior.Cascade); // Kaynak silinirse, beğenileri de silinsin 

        // TAKİP İLİŞKİSİ

        // Composite Key: Aynı kişiyi 2 kere takip edemezsin.
        modelBuilder.Entity<UserFollow>()
            .HasKey(uf => new { uf.FollowerId, uf.FollowingId });

        // Takip Eden (Ben) -> Takip Ettiklerim listesine bağlanır
        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(uf => uf.FollowerId)
            .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse veritabanı hata vermesin, biz yönetiriz.

        // Takip Edilen (Sen) -> Takipçiler listesine bağlanır
        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(uf => uf.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
 
 //   modelBuilder.Entity<Resource>().HasIndex(r => r.Url).IsUnique();

//     // KATEGORİ SEED DATA
//     modelBuilder.Entity<Category>().HasData(
//         new Category { Id = 1, Name = "Backend Geliştirme" },
//         new Category { Id = 2, Name = "Frontend Geliştirme" },
//         new Category { Id = 3, Name = "Siber Güvenlik" },
//         new Category { Id = 4, Name = "Veritabanı Sistemleri" },
//         new Category { Id = 5, Name = "Yapay Zeka ve Veri Bilimi" }
//     );
//     // KAYNAK (RESOURCE) SEED DATA - 5 Farklı Kategori İçin Örnekler
// modelBuilder.Entity<Resource>().HasData(
//     new Resource 
//     { 
//         Id = 1, 
//         Title = "Microsoft .NET Documentation", 
//         Url = "https://learn.microsoft.com/dotnet/", 
//         Description = "Kapsamlı .NET ve C# rehberi.", 
//         CategoryId = 1 // Backend Geliştirme
//     },
//     new Resource 
//     { 
//         Id = 2, 
//         Title = "React Official Docs", 
//         Url = "https://react.dev/", 
//         Description = "Modern Frontend geliştirme kılavuzu.", 
//         CategoryId = 2 // Frontend Geliştirme
//     },
//     new Resource 
//     { 
//         Id = 3, 
//         Title = "OWASP Top Ten", 
//         Url = "https://owasp.org/www-project-top-ten/", 
//         Description = "Web uygulama güvenliği için en kritik 10 risk listesi.", 
//         CategoryId = 3 // Siber Güvenlik
//     },
//     new Resource 
//     { 
//         Id = 4, 
//         Title = "PostgreSQL Tutorial", 
//         Url = "https://www.postgresqltutorial.com/", 
//         Description = "İleri seviye SQL ve DB yönetimi dersleri.", 
//         CategoryId = 4 // Veritabanı Sistemleri
//     },
//     new Resource 
//     { 
//         Id = 5, 
//         Title = "TensorFlow Hub", 
//         Url = "https://www.tensorflow.org/", 
//         Description = "Yapay zeka modelleri için açık kaynaklı kütüphane.", 
//         CategoryId = 5 // Yapay Zeka ve Veri Bilimi
//     }
// );
// 
}