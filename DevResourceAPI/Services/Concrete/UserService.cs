using DevResourceAPI.DTOs;
using DevResourceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevResourceAPI.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ServiceResult<(IEnumerable<UserDto> Data, int TotalRecords)>> GetAllUsersAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _userManager.Users.AsQueryable();

        // 1. ARAMA (Büyük 'N' ile UserName kullanıyoruz)
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u => u.UserName!.ToLower().Contains(search));
        }

        // 2. SAYFALAMA
        var totalRecords = await query.CountAsync();
        
        query = query.OrderByDescending(u => u.Id);

        if (pageSize > 0)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        // 3. DTO ÇEVİRİMİ
        var users = await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName!,
            })
            .ToListAsync();

        return ServiceResult<(IEnumerable<UserDto>, int)>.Ok((users, totalRecords));
    }
}