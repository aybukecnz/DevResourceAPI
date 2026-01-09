namespace DevResourceAPI.Models
{
    public class User
    {
        public string Role { get; set; } = "User"; // Varsayılan olarak herkes User
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }
}