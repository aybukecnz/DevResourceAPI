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

        // İlişki ayarların (Varsa buraya ekleyebilirsin, yoksa boş kalsın)
    }
}