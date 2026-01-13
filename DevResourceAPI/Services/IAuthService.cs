using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public interface IAuthService
{
    // Register: Service ile uyumlu
    Task<(bool Success, string Message)> RegisterAsync(UserRegisterDto request);

    // DÜZELTME 1: Service (bool, string) dönüyor, Interface de öyle olmalı.
    // Eskisi: (bool Success, string Message, string? Token) idi.
    Task<(bool Success, string Token)> LoginAsync(UserLoginDto request);

    //  DÜZELTME 2: Service 'string username' alıyor, Interface de öyle olmalı.
    // Eskisi: (int userId, string password) idi.
    Task<(bool Success, string Message)> DeleteAccountAsync(string username);

    // Tüm Kullanıcıları Listele (Manager için)
    Task<IEnumerable<UserDto>> GetAllUsersAsync();

    // Kullanıcıyı ID ile Sil (Manager için - Banlama)
    Task<(bool Success, string Message)> DeleteUserByIdAsync(int id);
}