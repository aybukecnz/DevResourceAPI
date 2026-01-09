
using DevResourceAPI.Models;
namespace DevResourceAPI.Services // Kendi namespace ismini kontrol et
{
    public interface IAuthService
    {
    Task<User>RegisterAsync(User user, string password);
    Task<string?> LoginAsync(string username, string password);
    Task<object?> GetProfileAsync(int userId);
    }
}