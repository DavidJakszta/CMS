namespace CMS.Server.Models;

public class SeedSettings
{
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminUserName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminDisplayName { get; set; } = "System Administrator";
}
