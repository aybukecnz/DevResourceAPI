using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Models;

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Identity ayarları için ZORUNLU

// --- TAKİP (FOLLOW) İLİŞKİSİ AYARLARI ---
    
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
    }

    
  
}