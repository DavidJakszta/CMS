namespace CMS.Server.Models.DTOs;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}
