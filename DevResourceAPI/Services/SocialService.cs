using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DevResourceAPI.Services;

public class SocialService : ISocialService
{
    private readonly AppDbContext _context;

    public SocialService(AppDbContext context)
    {
        _context = context;
    }

    // --- BEĞENME İŞLEMİ ---
    public async Task<ServiceResult> ToggleResourceLikeAsync(int resourceId, int userId)
    {
        var resource = await _context.Resources.FindAsync(resourceId);
        if (resource == null) return ServiceResult.Fail("Kaynak bulunamadı.");

        var existingLike = await _context.ResourceLikes
            .FirstOrDefaultAsync(x => x.ResourceId == resourceId && x.UserId == userId);

        if (existingLike != null)
        {
            _context.ResourceLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok("Beğeni geri alındı. 💔");
        }
        else
        {
            // CreatedAt ATAMASI SİLİNDİ (BaseEntity hallediyor)
            var newLike = new ResourceLike { ResourceId = resourceId, UserId = userId };
            _context.ResourceLikes.Add(newLike);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok("Kaynak beğenildi! ❤️");
        }
    }

    // --- TAKİP İŞLEMİ ---
    public async Task<ServiceResult> ToggleUserFollowAsync(string targetUserName, int currentUserId)
    {
        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == targetUserName);
        if (targetUser == null) return ServiceResult.Fail("Kullanıcı bulunamadı.");

        if (targetUser.Id == currentUserId) return ServiceResult.Fail("Kendinizi takip edemezsiniz!");

        var existingFollow = await _context.UserFollows
            .FirstOrDefaultAsync(x => x.FollowerId == currentUserId && x.FollowingId == targetUser.Id);

        if (existingFollow != null)
        {
            _context.UserFollows.Remove(existingFollow);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok($"{targetUserName} takipten çıkarıldı.");
        }
        else
        {
            // CreatedAt ATAMASI SİLİNDİ (BaseEntity hallediyor)
            var newFollow = new UserFollow { FollowerId = currentUserId, FollowingId = targetUser.Id };
            _context.UserFollows.Add(newFollow);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok($"{targetUserName} takip edildi! 🚀");
        }
    }
}