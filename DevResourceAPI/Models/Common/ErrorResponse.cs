namespace DevResourceAPI.Models;

public class ErrorResponse: BaseEntity
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Detailed { get; set; } 
}