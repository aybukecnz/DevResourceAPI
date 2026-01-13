namespace DevResourceAPI.DTOs;

public class ResourceDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; } // Bu eksikti, hata veriyordu
    public string CategoryName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int LikeCount { get; set; }      
    public bool IsLikedByMe { get; set; }
}