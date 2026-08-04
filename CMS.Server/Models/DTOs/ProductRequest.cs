namespace CMS.Server.Models.DTOs;

public class ProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
}
