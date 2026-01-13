using System.ComponentModel.DataAnnotations;

namespace DevResourceAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // default role

//      public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
//      public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
//      ESKİDEN: public byte[] PasswordHash { get; set; }
//      YENİSİ: String olmalı çünkü BCrypt string döner!
    public virtual ICollection<Category>? Categories { get; set; }
    public virtual ICollection<Resource>? Resources { get; set; }

    // Nullable (?) değil, direkt boş liste olarak başlatıyoruz.
    // Böylece "user.LikedResources" asla NULL gelmez, en kötü ihtimalle BOŞ gelir.
    public ICollection<ResourceLike> LikedResources { get; set; } = new List<ResourceLike>();

    public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>(); 
    
    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
    }
}