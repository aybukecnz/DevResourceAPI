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

    //  BEĞENME İŞLEMİ 
    public async Task<(bool Success, string Message)> ToggleResourceLikeAsync(int resourceId, int userId)
    {
        // Kaynak var mı?
        var resource = await _context.Resources.FindAsync(resourceId);
        if (resource == null) return (false, "Kaynak bulunamadı.");

        // Zaten beğenmiş mi?
        var existingLike = await _context.ResourceLikes
            .FirstOrDefaultAsync(x => x.ResourceId == resourceId && x.UserId == userId);

        if (existingLike != null)
        {
            // Zaten beğenmiş -> O zaman beğeniyi kaldır (Unlike)
            _context.ResourceLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return (true, "Beğeni geri alındı. 💔");
        }
        else
        {
            // Beğenmemiş -> Yeni beğeni ekle (Like)
            var newLike = new ResourceLike { ResourceId = resourceId, UserId = userId };
            _context.ResourceLikes.Add(newLike);
            await _context.SaveChangesAsync();
            return (true, "Kaynak beğenildi! ❤️");
        }
    }

    //  TAKİP İŞLEMİ 
    public async Task<(bool Success, string Message)> ToggleUserFollowAsync(string targetUserName, int currentUserId)
    {
        // İsmi verilen kullanıcıyı bul
        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == targetUserName);
        
        if (targetUser == null) 
            return (false, "Böyle bir kullanıcı bulunamadı.");

        // Kendini takip edemezsin
        if (targetUser.Id == currentUserId) 
            return (false, "Kendinizi takip edemezsiniz!");

        //İlişkiyi kontrol et (Burada ID kullan çünkü veritabanı ID sever)
        var existingFollow = await _context.UserFollows
            .FirstOrDefaultAsync(x => x.FollowerId == currentUserId && x.FollowingId == targetUser.Id);

        if (existingFollow != null)
        {
            _context.UserFollows.Remove(existingFollow);
            await _context.SaveChangesAsync();
            return (true, $"{targetUserName} takipten çıkarıldı.");
        }
        else
        {
            var newFollow = new UserFollow { FollowerId = currentUserId, FollowingId = targetUser.Id };
            _context.UserFollows.Add(newFollow);
            await _context.SaveChangesAsync();
            return (true, $"{targetUserName} takip edildi! 🚀");
        }
    }
}