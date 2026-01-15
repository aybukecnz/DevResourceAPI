namespace DevResourceAPI.DTOs;
using DevResourceAPI.Models.Common;

public class UserDto : BaseEntity
{
    public int Id { get; set; } = 0;           
    public string UserName { get; set; }= string.Empty;
    public string Password { get; set; } = string.Empty;  
    public string Role { get; set; } = string.Empty;    
}