using System.Security.Claims;

namespace DevResourceAPI.Extensions
{
    public static class ClaimsExtensions
    {
        // Token içindeki ID'yi güvenli şekilde okur
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            // Eğer claim yoksa veya null ise 0 döner, hata patlatmaz.
            return idClaim != null && int.TryParse(idClaim.Value, out int id) ? id : 0;
        }

        // Token içindeki Rolü okur
        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? "User"; // Rol yoksa varsayılan User
        }

        // Token içindeki Kullanıcı Adını okur
        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.Identity?.Name ?? "Bilinmeyen Kullanıcı";
        }
    }
}