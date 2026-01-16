using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DevResourceAPI.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ResourceLike> ResourceLikes { get; set; }
    // DbSet<User> gerekmez, IdentityDbContext halleder.
    public DbSet<UserFollow> UserFollows { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // O specific sarı uyarıyı (Global Query Filter uyarısı) susturuyorum.
    // Çünkü kod tarafında dolu kategorinin silinmesini zaten engelledim.
    optionsBuilder.ConfigureWarnings(warnings => 
        warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
    
    base.OnConfiguring(optionsBuilder);
}
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Identity ayarları için ZORUNLU

    // GLOBAL QUERY FILTER (SOFT DELETE KORUMASI) 
    // Artık her sorguda "IsDeleted == false" şartı otomatik eklenecek.
    
    // 1. Kategoriler için koruma
    builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);

    // 2. Kaynaklar için koruma
    builder.Entity<Resource>().HasQueryFilter(r => !r.IsDeleted);

    // (Eğer User veya başka tablolarında da Soft Delete varsa onları da buraya ekle)
    // modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

    base.OnModelCreating(builder);
// --- FOLLOW İLİŞKİSİ AYARLARI ---
    
    // İki anahtarın birleşiminden oluşan bir "Composite Key" yapıyoruz.
    // Yani aynı kişi, aynı kişiyi ikinci kez takip edemesin.
    builder.Entity<UserFollow>()
        .HasKey(uf => new { uf.FollowerId, uf.FollowingId });

    // Takip Eden (Follower) İlişkisi
    builder.Entity<UserFollow>()
        .HasOne(uf => uf.Follower)
        .WithMany(u => u.Following) // User'daki "Following" listesine bağlanır
        .HasForeignKey(uf => uf.FollowerId)
        .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse takip verisi hemen silinmesin (Hata önlemek için)

    // Takip Edilen (Followee) İlişkisi
    builder.Entity<UserFollow>()
        .HasOne(uf => uf.Following)
        .WithMany(u => u.Followers) // User'daki "Followers" listesine bağlanır
        .HasForeignKey(uf => uf.FollowingId)
        .OnDelete(DeleteBehavior.Restrict);

//--- BEĞENİ (RESOURCE LIKE) AYARLARI ---
    
    // Aynı kullanıcı aynı kaynağı 2 kere beğenemesin diye Composite Key yapıyoruz:
    builder.Entity<ResourceLike>()
        .HasKey(rl => new { rl.UserId, rl.ResourceId });

    // İlişkileri de sağlamlaştıralım:
    builder.Entity<ResourceLike>()
        .HasOne(rl => rl.Resource)
        .WithMany(r => r.Likes)
        .HasForeignKey(rl => rl.ResourceId)
        .OnDelete(DeleteBehavior.Cascade); // Kaynak silinirse beğenisi de silinsin

    builder.Entity<ResourceLike>()
        .HasOne(rl => rl.User)
        .WithMany(u => u.LikedResources)
        .HasForeignKey(rl => rl.UserId)
        .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse hata vermesin (veya Cascade yapabilirsin)

    // KATEGORİ İÇİN GÖRÜNMEZLİK FİLTRESİ
    builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
    
    // Eğer User tablosunda da varsa onun için de yapabilirsin:
    // modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

    base.OnModelCreating(builder);    
    }

    
  
}