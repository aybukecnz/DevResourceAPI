namespace DevResourceAPI.DTOs;

// <T> demek: İçine ister Kategori, ister Ürün, ister Kullanıcı koyabilirsin demek.
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } // Verilerimiz
    public int TotalRecords { get; set; }     // Toplam Sayı

    public PagedResult(IEnumerable<T> items, int totalRecords)
    {
        Items = items;
        TotalRecords = totalRecords;
    }
}