namespace DevResourceAPI.Models;

public class ResourceLike
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    // Ne zaman beğendi?
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}