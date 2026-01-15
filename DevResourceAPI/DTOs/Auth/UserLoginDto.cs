using System.ComponentModel.DataAnnotations;

namespace DevResourceAPI.DTOs;

public class UserLoginDto
{
    [Required]
    public string UserName { get; set; } = string.Empty; 

    [Required]
    public string Password { get; set; } = string.Empty;
}