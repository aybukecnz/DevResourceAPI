namespace DevResourceAPI.DTOs;

// <T> generic tipi temsil eder
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } 
    public int TotalRecords { get; set; }  
    public int PageNumber { get; set; } 
    public int PageSize { get; set; }  
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public string PageInfo => $"{PageNumber}/{TotalPages}";

    // Frontend için butonları aç/kapa bilgisi
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public PagedResult(IEnumerable<T> items, int totalRecords, int pageNumber, int pageSize)
    {
        Items = items;
        TotalRecords = totalRecords;
        PageNumber = pageNumber; 
        PageSize = pageSize;
    }
}