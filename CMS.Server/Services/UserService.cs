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

        public async Task<RegisterResult> CreateUserAsync(RegisterRequest request)
        {
            var errors = new List<string>();

            var emailExists = await _userManager.Users
                .AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                errors.Add("Email is already in use.");

            var displayNameExists = await _userManager.Users
                .AnyAsync(u => u.DisplayName == request.DisplayName);
            if (displayNameExists)
                errors.Add("Display name is already in use.");

            if (errors.Count > 0)
                return new RegisterResult { Success = false, Errors = errors };

            var userNameTaken = await _userManager.FindByNameAsync(request.UserName);
            if (userNameTaken is not null)
            {
                var suggested = await GenerateSuggestedUserNameAsync(request.UserName);
                return new RegisterResult
                {
                    Success = false,
                    Errors = ["Username is already taken."],
                    SuggestedUserName = suggested
                };
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                DisplayName = request.DisplayName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e => e.Description));
                return new RegisterResult { Success = false, Errors = errors };
            }

            return new RegisterResult
            {
                Success = true,
                User = MapToResponse(user)
            };
        }

        private async Task<string> GenerateSuggestedUserNameAsync(string baseName)
        {
            var random = Random.Shared;
            for (var i = 0; i < 10; i++)
            {
                var suggestion = $"{baseName}{random.Next(100, 999)}";
                var exists = await _userManager.FindByNameAsync(suggestion);
                if (exists is null)
                    return suggestion;
            }
            return $"{baseName}{Guid.NewGuid().ToString("N")[..6]}";
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

        private static UserResponse MapToResponse(ApplicationUser user)
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
