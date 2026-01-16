namespace DevResourceAPI.DTOs;

public class ResourceDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; } 
    public string CategoryName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int LikeCount { get; set; }      
    public bool IsLikedByMe { get; set; }
    public string CreatedBy { get; set; } = string.Empty;  
    public DateTime CreatedAt { get; set; } 
}