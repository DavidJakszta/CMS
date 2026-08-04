namespace CMS.Server.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }
}
