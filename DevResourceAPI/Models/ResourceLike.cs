namespace DevResourceAPI.Models;

public class ResourceLike : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

}