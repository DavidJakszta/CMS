using CMS.Server.Models.DTOs;

namespace CMS.Server.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> CreateUserAsync(RegisterRequest request);
        Task<UserResponse?> GetUserByIdAsync(int id);
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int id);
    }
}
