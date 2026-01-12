namespace DevResourceAPI.DTOs;

public class UserGroupedResourceDto
{
    public string OwnerName { get; set; } = string.Empty; 
    public List<ResourceDto> Resources { get; set; } = new List<ResourceDto>(); 
}