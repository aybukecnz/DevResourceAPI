using DevResourceAPI.Models.Common;

namespace DevResourceAPI.Models;
public class ErrorLog : BaseEntity
{
    public int Id { get; set; }
    public string RequestPath { get; set; } = string.Empty;
    public string RequestMethod { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
}