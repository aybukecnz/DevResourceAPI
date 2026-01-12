namespace DevResourceAPI.Models;

public class UserFollow
{
    // Takip eden kişi (Ben)
    public int FollowerId { get; set; }
    public User Follower { get; set; }  = null!;

    // Takip edilen kişi (Sen)
    public int FollowingId { get; set; }
    public User Following { get; set; } = null!;
    
    // Ne zaman takip etti?
    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
}