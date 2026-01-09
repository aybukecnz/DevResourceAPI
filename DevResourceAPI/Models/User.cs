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
        public string Role { get; set; } = "User"; // Varsayılan olarak herkes User
//       public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
//       public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
// ESKİDEN: public byte[] PasswordHash { get; set; }
// YENİSİ: String olmalı çünkü BCrypt string döner!
        public virtual ICollection<Category>? Categories { get; set; }
        public virtual ICollection<Resource>? Resources { get; set; }
    }
}