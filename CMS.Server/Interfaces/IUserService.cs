using CMS.Server.Models.DTOs;

namespace CMS.Server.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResult> CreateUserAsync(RegisterRequest request);
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<UserResponse?> GetUserByIdAsync(int id);
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AssignRoleAsync(int userId, string role);
        Task<List<string>> GetUserRolesAsync(int userId);
    }
}
