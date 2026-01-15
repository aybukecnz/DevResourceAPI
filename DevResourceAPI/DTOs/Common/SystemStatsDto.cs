namespace DevResourceAPI.DTOs;

public class SystemStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalResources { get; set; }
    public int TotalCategories { get; set; }
    // Bu raporun oluşturulduğu anı gösterir (DB'ye kaydedilmez) baseEntity kullanmama sebebim bu
    public DateTime LastUpdate { get; set; } 
}