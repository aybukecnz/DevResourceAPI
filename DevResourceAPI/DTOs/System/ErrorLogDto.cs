namespace DevResourceAPI.DTOs;

public class ErrorLogDto : BaseEntity
{
    public int Id { get; set; }
    
    // Hata nerede oldu? (/api/categories vb.)
    public string RequestPath { get; set; } = string.Empty;
    
    // İstek tipi neydi? (GET, POST, DELETE)
    public string RequestMethod { get; set; } = string.Empty;
    
    // Hatanın kısa özeti (Object reference not set...)
    public string ErrorMessage { get; set; } = string.Empty;

}