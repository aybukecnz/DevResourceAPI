namespace DevResourceAPI.DTOs;

public class CreateCategoryDto
{
    // Kullanıcıdan sadece kategori ismini istiyoruz.
    public string Name { get; set; } = string.Empty;
}