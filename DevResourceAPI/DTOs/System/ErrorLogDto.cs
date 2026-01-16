namespace DevResourceAPI.DTOs;

public class ErrorLogDto : BaseEntity
{
    public int Id { get; set; }
    
    public string RequestPath { get; set; } = string.Empty;
    
    public string RequestMethod { get; set; } = string.Empty;
    
    public string ErrorMessage { get; set; } = string.Empty;

}