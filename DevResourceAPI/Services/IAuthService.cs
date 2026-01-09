using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(UserRegisterDto request);
    Task<(bool Success, string Message, string? Token)> LoginAsync(UserLoginDto request);
    Task<(bool Success, string Message)> DeleteAccountAsync(int userId, string password);
}