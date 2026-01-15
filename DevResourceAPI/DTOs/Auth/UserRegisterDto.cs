using System.ComponentModel.DataAnnotations;

namespace DevResourceAPI.DTOs;

public class UserRegisterDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
}