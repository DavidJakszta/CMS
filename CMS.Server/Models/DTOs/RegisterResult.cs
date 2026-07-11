namespace CMS.Server.Models.DTOs;

public class RegisterResult
{
    public bool Success { get; set; }
    public UserResponse? User { get; set; }
    public string? SuggestedUserName { get; set; }
    public List<string> Errors { get; set; } = [];
}
