namespace DevResourceAPI.DTOs;

public class UpdateResourceDto
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}