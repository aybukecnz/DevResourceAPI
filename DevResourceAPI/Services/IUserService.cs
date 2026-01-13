using DevResourceAPI.DTOs;

namespace DevResourceAPI.Services;

public interface IUserService
{
    Task<(IEnumerable<UserDto> Data, int TotalRecords)> GetAllUsersAsync(string? search, int pageNumber, int pageSize);
}