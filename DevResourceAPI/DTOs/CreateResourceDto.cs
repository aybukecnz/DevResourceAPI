namespace DevResourceAPI.DTOs;

public class CreateResourceDto
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    // Kullanıcı kaynağı eklerken hangi kategoriye ait olduğunu söylemeli
    public int CategoryId { get; set; } 
}