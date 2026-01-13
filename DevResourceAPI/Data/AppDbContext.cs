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
 
 
}