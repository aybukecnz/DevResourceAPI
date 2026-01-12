namespace DevResourceAPI.Services;

public interface ISocialService
{
    // Kaynağı Beğen / Vazgeç (Toggle mantığı: Varsa siler, yoksa ekler)
    Task<(bool Success, string Message)> ToggleResourceLikeAsync(int resourceId, int userId);

    // Kullanıcıyı Takip Et / Bırak
    Task<(bool Success, string Message)> ToggleUserFollowAsync(string targetUsername, int currentUserId);
}