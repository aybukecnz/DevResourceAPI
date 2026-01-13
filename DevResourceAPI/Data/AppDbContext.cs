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

        
        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(uf => uf.FollowerId)
            .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse veritabanı hata vermesin, biz yönetiriz.

        
        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(uf => uf.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    //BaseEntity otomasyonu
    // Senkron kayıtlar için
        public override int SaveChanges()
    {
        SetBaseEntityDates();
        return base.SaveChanges();              
    }
    // Asenkron kayıtlar için
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetBaseEntityDates();
        return await base.SaveChangesAsync(cancellationToken);
    }
    //ortak tarih atama mantığı
    private void SetBaseEntityDates()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Yeni ekleniyorsa CreatedDate ata
                    // PostgreSQL için UTC kullanmak en sağlıklı standarttır
                    entry.Entity.CreatedAt = DateTime.UtcNow; 
                    break;

                case EntityState.Modified:
                    // Güncelleniyorsa UpdatedDate ata
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    
}
}