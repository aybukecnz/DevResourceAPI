namespace DevResourceAPI.DTOs;
public class ResourceDto
{
    public int Id {get; set;}
    public string Title {get; set;} = string.Empty;
    public string Url {get; set;} = string.Empty;
    public string CategoryName {get; set;} = string.Empty;
}