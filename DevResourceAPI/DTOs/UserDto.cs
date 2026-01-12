namespace DevResourceAPI.DTOs;

public class UserDto
{
    public int Id { get; set; } = 0;           
    public string Username { get; set; }= string.Empty;
    public string Password { get; set; } = string.Empty;  
    public string Role { get; set; } = string.Empty;    
}