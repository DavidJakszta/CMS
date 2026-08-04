using CMS.Server.Models;
using CMS.Server.Models.DTOs;

namespace CMS.Server.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResult> CreateUserAsync(RegisterRequest request);
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<UserResponse?> GetUserByIdAsync(int id, RequestContext requester);
        Task<List<UserResponse>> GetAllUsersAsync(RequestContext requester);
        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request, RequestContext requester);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AssignRoleAsync(int userId, string role);
        Task<List<string>> GetUserRolesAsync(int userId);
    }
}
