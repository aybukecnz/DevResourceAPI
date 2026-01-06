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
}