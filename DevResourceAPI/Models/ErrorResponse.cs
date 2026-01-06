namespace DevResourceAPI.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Detailed { get; set; } // Sadece geliştirme modunda görünecek
}