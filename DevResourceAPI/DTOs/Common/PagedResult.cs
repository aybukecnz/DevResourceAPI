namespace DevResourceAPI.DTOs;

// <T> generic tipi temsil eder
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } 
    public int TotalRecords { get; set; }     

    public PagedResult(IEnumerable<T> items, int totalRecords)
    {
        Items = items;
        TotalRecords = totalRecords;
    }
}