using DevResourceAPI.Models;

namespace DevResourceAPI.Services;

public interface ISocialService
{
    Task<ServiceResult> ToggleResourceLikeAsync(int resourceId, int userId);
    Task<ServiceResult> ToggleUserFollowAsync(string targetUsername, int currentUserId);
}