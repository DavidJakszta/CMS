using Microsoft.AspNetCore.Identity;

namespace CMS.Server.Models;

public class ApplicationUser : IdentityUser<int>
{
    string DisplayName { get; set; } = string.Empty;
}