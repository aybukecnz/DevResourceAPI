using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(UserRegisterDto request);
    Task<(bool Success, string Message, string? Token)> LoginAsync(UserLoginDto request);
    Task<(bool Success, string Message)> DeleteAccountAsync(int userId, string password);
    // Tüm Kullanıcıları Listele (Manager için)
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    // Kullanıcıyı ID ile Sil (Manager için - Banlama)
    Task<(bool Success, string Message)> DeleteUserByIdAsync(int id);
}