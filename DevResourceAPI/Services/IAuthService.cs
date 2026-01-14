using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public interface IAuthService
{
    // Eski tuple hali: Task<(bool Success, string Message)> RegisterAsync(UserRegisterDto request);
    Task<ServiceResult> RegisterAsync(UserRegisterDto request);
    //Task<(bool Success, string Token)> LoginAsync(UserLoginDto request);
    Task<ServiceResult<string>> LoginAsync(UserLoginDto request);
    Task<ServiceResult> DeleteAccountAsync(string username);
    // Tüm Kullanıcıları Listele (Manager için)
    //Task<IEnumerable<UserDto>> GetAllUsersAsync(); burda direkt liste görünüyordu, şimdi kutuya koyulacak
    Task<ServiceResult<IEnumerable<UserDto>>> GetAllUsersAsync();
    // Kullanıcıyı ID ile Sil (Manager için - Banlama)
    Task<ServiceResult> DeleteUserByIdAsync(int id);
}