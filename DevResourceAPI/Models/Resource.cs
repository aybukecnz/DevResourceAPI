using System.ComponentModel.DataAnnotations;

namespace DevResourceAPI.Models;

public class Resource
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık alanı boş bırakılamaz.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Başlık en az 3, en fazla 100 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL zorunludur.")]
    [Url(ErrorMessage = "Lütfen geçerli bir URL adresi giriniz (http:// veya https:// ile başlamalı).")]
    public string Url { get; set; } = string.Empty;
    [MaxLength(500, ErrorMessage = "Açıklama 500 karakterden uzun olamaz.")]
    public string? Description { get; set; }
    // İlişkisel kural
    [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir kategori seçiniz.")]
    
    public int CategoryId { get; set; }
     public virtual Category? Category { get; set; }
    
    public int UserId { get; set; }
    public virtual User? User { get; set; } 
   
}