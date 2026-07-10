using CMS.Server.Interfaces;
using CMS.Server.Models;
using CMS.Server.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMS.Server.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserResponse> CreateUserAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                DisplayName = request.DisplayName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            return MapToResponse(user);
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            return user is null ? null : MapToResponse(user);
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var responses = new List<UserResponse>();
            foreach (var user in users)
            {
                responses.Add(MapToResponse(user));
            }
            return responses;
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return null;

            if (request.UserName is not null)
                user.UserName = request.UserName;
            if (request.Email is not null)
                user.Email = request.Email;
            if (request.DisplayName is not null)
                user.DisplayName = request.DisplayName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User update failed: {errors}");
            }

            return MapToResponse(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        private UserResponse MapToResponse(ApplicationUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName
            };
        }
    }
}
