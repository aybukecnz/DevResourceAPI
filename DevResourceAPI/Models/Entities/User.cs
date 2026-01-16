using Microsoft.AspNetCore.Identity;

namespace DevResourceAPI.Models;

public class User : IdentityUser<int>
{
    // IdentityUser zaten Id, UserName, Email, PasswordHash içeriyor.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } 

    public string Role { get; set; } = "User";

    // İlişkiler
    public virtual ICollection<Category>? Categories { get; set; }
    public virtual ICollection<Resource>? Resources { get; set; }
    public ICollection<ResourceLike> LikedResources { get; set; } = new List<ResourceLike>();
    public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
}