using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMS.Server.Interfaces;
using CMS.Server.Models;
using CMS.Server.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CMS.Server.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly JwtSettings _jwt;
        private readonly IProductService _productService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<JwtSettings> jwt,
            IProductService productService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
            _productService = productService;
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

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new LoginResult
                {
                    Success = false,
                    Errors = ["Invalid username or password."]
                };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var loginResult = new LoginResult
            {
                Success = true,
                User = MapToResponse(user)
            };
            loginResult.User.Roles = roles.ToList();

            if (request.RequestToken)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.UserName ?? ""),
                    new(ClaimTypes.Email, user.Email ?? "")
                };
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwt.Issuer,
                    audience: _jwt.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwt.ExpireMinutes),
                    signingCredentials: creds);

                loginResult.Token = new JwtSecurityTokenHandler().WriteToken(token);
            }

            return loginResult;
        }

        public async Task<bool> AssignRoleAsync(int userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return false;

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return [];

            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id, RequestContext requester)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return null;

            var response = MapToResponse(user, requester);
            response.ProductCount = await _productService.GetProductCountAsync(user.Id);
            return response;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync(RequestContext requester)
        {
            var users = await _userManager.Users.ToListAsync();
            var productCounts = await _productService.GetProductCountsAsync();

            return users
                .Select(u =>
                {
                    var response = MapToResponse(u, requester);
                    response.ProductCount = productCounts.GetValueOrDefault(u.Id);
                    return response;
                })
                .OrderByDescending(u => u.ProductCount)
                .ThenBy(u => u.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request, RequestContext requester)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return null;

            if (!CanModify(id, requester))
                throw new UnauthorizedAccessException();

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

            return MapToResponse(user, requester);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        private static bool CanModify(int targetUserId, RequestContext requester)
        {
            return requester.IsAdmin || requester.UserId == targetUserId;
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

        private static UserResponse MapToResponse(ApplicationUser user, RequestContext requester)
        {
            var isSelfOrAdmin = requester.IsAdmin || requester.UserId == user.Id;
            return new UserResponse
            {
                Id = user.Id,
                UserName = isSelfOrAdmin ? user.UserName ?? string.Empty : string.Empty,
                Email = isSelfOrAdmin ? user.Email ?? string.Empty : string.Empty,
                DisplayName = user.DisplayName
            };
        }
    }
}
