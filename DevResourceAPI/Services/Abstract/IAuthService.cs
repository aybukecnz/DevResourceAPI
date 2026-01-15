using DevResourceAPI.DTOs;
using DevResourceAPI.Models.Common;    

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
    Task<ServiceResult<PagedResult<UserDto>>> GetAllUsersAsync(int pageNumber, int pageSize);
    // Kullanıcıyı ID ile Sil (Manager için - Banlama)
    Task<ServiceResult> DeleteUserByIdAsync(int id);    
    // İstatistikler
    Task<ServiceResult<SystemStatsDto>> GetSystemStatsAsync();
    Task<ServiceResult<IEnumerable<ErrorLogDto>>> GetErrorLogsAsync();
}