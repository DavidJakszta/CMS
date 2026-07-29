namespace CMS.Server.Models.DTOs;

public class LoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserResponse? User { get; set; }
    public List<string> Errors { get; set; } = [];
}
