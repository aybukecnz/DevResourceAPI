using System.ComponentModel.DataAnnotations;
namespace DevResourceAPI.Models;

public class Category
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [MinLength(2, ErrorMessage = "Kategori adı en az 2 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    // Relation: Bir kategorinin birden fazla kaynağı olabilir (1-N)
    public int UserId { get; set; } 
    public ICollection<Resource> Resources { get; set; } = new HashSet<Resource>();
    public virtual User User { get; set; } = null!;
}
