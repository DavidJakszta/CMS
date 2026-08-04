using Bogus;
using CMS.Server.DB;
using CMS.Server.Interfaces;
using CMS.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CMS.Server.Services
{
    public class DataSeeder : IDataSeeder
    {
        private const string DummyPassword = "Password123!";
        private const string AdminRole = "Admin";
        private const int UserCount = 100;
        private const int ProductCount = 100;

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IOptions<SeedSettings> _seed;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<SeedSettings> seed,
            ILogger<DataSeeder> logger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _seed = seed;
            _logger = logger;
        }

        public async Task WipeAndSeedAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_db.Database.GetDbConnection().ConnectionString))
                throw new InvalidOperationException(
                    "Cannot seed: 'ConnectionStrings:DefaultConnection' is not configured. " +
                    "Set it via the ConnectionStrings__DefaultConnection environment variable.");

            ValidateAdminSettings();

            await WipeAsync(cancellationToken);

            _logger.LogInformation(
                "Seeding {UserCount} users and {ProductCount} products...", UserCount, ProductCount);
            var users = await SeedUsersAsync(cancellationToken);
            await SeedProductsAsync(users, cancellationToken);

            _logger.LogInformation(
                "Done. {UserCount} users and {ProductCount} products. Dummy users share the dev password '{DummyPassword}'; " +
                "the admin account is created from the Seed configuration.",
                users.Count, ProductCount, DummyPassword);
        }

        private void ValidateAdminSettings()
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(_seed.Value.AdminPassword))
                missing.Add("Seed:AdminPassword");
            if (string.IsNullOrWhiteSpace(_seed.Value.AdminUserName))
                missing.Add("Seed:AdminUserName");
            if (string.IsNullOrWhiteSpace(_seed.Value.AdminEmail))
                missing.Add("Seed:AdminEmail");

            if (missing.Count > 0)
                throw new InvalidOperationException(
                    $"Cannot seed: admin account is not configured. Set {string.Join(", ", missing)} " +
                    "(e.g. via the Seed__AdminPassword, Seed__AdminUserName and Seed__AdminEmail environment variables).");
        }

        private async Task WipeAsync(CancellationToken cancellationToken)
        {
            await _db.Database.MigrateAsync(cancellationToken);

            await _db.Products.ExecuteDeleteAsync(cancellationToken);
            await _db.UserRoles.ExecuteDeleteAsync(cancellationToken);
            await _db.UserClaims.ExecuteDeleteAsync(cancellationToken);
            await _db.UserLogins.ExecuteDeleteAsync(cancellationToken);
            await _db.UserTokens.ExecuteDeleteAsync(cancellationToken);
            await _db.Users.ExecuteDeleteAsync(cancellationToken);
            await _db.Roles.ExecuteDeleteAsync(cancellationToken);
        }

        private async Task<List<ApplicationUser>> SeedUsersAsync(CancellationToken cancellationToken)
        {
            var faker = new Faker<ApplicationUser>()
                .RuleFor(u => u.UserName, f => f.Internet.UserName().ToLowerInvariant())
                .RuleFor(u => u.Email, f => f.Internet.Email(f.Person.FirstName, f.Person.LastName))
                .RuleFor(u => u.DisplayName, f => f.Name.FullName())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.EmailConfirmed, true);

            var seenUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenDisplayNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var users = new List<ApplicationUser>();
            foreach (var user in faker.Generate(UserCount))
            {
                user.UserName = EnsureUnique(user.UserName, seenUserNames);
                user.Email = EnsureUnique(user.Email, seenEmails);
                user.DisplayName = EnsureUnique(user.DisplayName, seenDisplayNames);

                var result = await _userManager.CreateAsync(user, DummyPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        $"Failed to create user '{user.Email}': {string.Join("; ", result.Errors)}");

                users.Add(user);
            }

            var admin = await CreateAdminAsync();
            if (admin is not null)
                users.Add(admin);

            return users;
        }

        private async Task<ApplicationUser?> CreateAdminAsync()
        {
            await EnsureRoleAsync(AdminRole);

            var existing = await _userManager.FindByEmailAsync(_seed.Value.AdminEmail);
            if (existing is not null)
                return existing;

            var admin = new ApplicationUser
            {
                UserName = _seed.Value.AdminUserName,
                Email = _seed.Value.AdminEmail,
                DisplayName = _seed.Value.AdminDisplayName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, _seed.Value.AdminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create admin user: {string.Join("; ", result.Errors)}");

            result = await _userManager.AddToRoleAsync(admin, AdminRole);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to assign '{AdminRole}' role: {string.Join("; ", result.Errors)}");

            return admin;
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            if (await _roleManager.FindByNameAsync(roleName) is not null)
                return;

            var result = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {string.Join("; ", result.Errors)}");
        }

        private async Task SeedProductsAsync(
            IReadOnlyList<ApplicationUser> users, CancellationToken cancellationToken)
        {
            var faker = new Faker();
            var allUsers = users.ToList();
            var powerSellers = faker.PickRandom(allUsers, Math.Min(15, allUsers.Count)).ToList();

            var products = new List<Product>(ProductCount);
            for (var i = 0; i < ProductCount; i++)
            {
                var adjective = faker.Commerce.ProductAdjective();
                var material = faker.Commerce.ProductMaterial();
                var noun = faker.Commerce.ProductName().Split(' ').Last();
                var name = $"{adjective} {material} {noun}";
                var owner = faker.Random.Int(1, 100) <= 70
                    ? faker.PickRandom(powerSellers)
                    : faker.PickRandom(allUsers);

                products.Add(new Product
                {
                    Name = name,
                    Price = faker.Finance.Amount(1, 2000, 2),
                    Description = BuildDescription(faker, adjective, material, noun),
                    PictureUrl = $"https://loremflickr.com/600/400/{noun.ToLowerInvariant()}?lock={StableHash(name)}",
                    Owner = owner
                });
            }

            await _db.Products.AddRangeAsync(products, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        private static string BuildDescription(Faker faker, string adjective, string material, string noun)
        {
            var adjectiveLower = adjective.ToLowerInvariant();
            var materialLower = material.ToLowerInvariant();
            var nounLower = noun.ToLowerInvariant();
            var plural = IsPluralNoun(noun);
            var article = "aeiou".IndexOf(adjectiveLower[0]) >= 0 ? "an" : "a";

            string[] leads;
            string[] follows;
            if (plural)
            {
                leads =
                [
                    $"These {nounLower} are crafted from premium {materialLower}.",
                    $"Made from high-quality {materialLower}, these {nounLower} are built to last.",
                    $"These {adjectiveLower} {nounLower} combine style with everyday practicality.",
                    $"{Capitalize(adjectiveLower)} {nounLower} designed for comfort and durability."
                ];
                follows =
                [
                    $"Their {materialLower} construction makes them {article} {adjectiveLower} choice for everyday use.",
                    $"Lightweight yet {adjectiveLower}, they fit perfectly into any home or office.",
                    $"Customers appreciate how {adjectiveLower} and versatile these {nounLower} are.",
                    $"Ideal for daily life, they stay {adjectiveLower} no matter the occasion.",
                    $"One of our most popular items, these {nounLower} deliver on quality every time."
                ];
            }
            else
            {
                leads =
                [
                    $"The {nounLower} is crafted from premium {materialLower}.",
                    $"Made from high-quality {materialLower}, this {nounLower} is built to last.",
                    $"This {adjectiveLower} {nounLower} combines style with everyday practicality.",
                    $"{article} {adjectiveLower} {nounLower} designed for comfort and durability."
                ];
                follows =
                [
                    $"Its {materialLower} construction makes it {article} {adjectiveLower} choice for everyday use.",
                    $"Lightweight yet {adjectiveLower}, it fits perfectly into any home or office.",
                    $"Customers appreciate how {adjectiveLower} and versatile this {nounLower} is.",
                    $"Ideal for daily life, it stays {adjectiveLower} no matter the occasion.",
                    $"One of our most popular items, this {nounLower} delivers on quality every time."
                ];
            }

            var text = $"{faker.PickRandom(leads)} {faker.PickRandom(follows)}";
            return Capitalize(text);
        }

        private static readonly HashSet<string> SingularNounsEndingInS = new(StringComparer.OrdinalIgnoreCase)
        {
            "cheese", "fish", "gas", "bus", "canvas", "glass", "lens",
            "grass", "address", "class", "mass", "basis", "analysis"
        };

        private static bool IsPluralNoun(string noun)
        {
            var lower = noun.ToLowerInvariant();
            return !SingularNounsEndingInS.Contains(lower) && lower.EndsWith('s');
        }

        private static string Capitalize(string value)
        {
            return char.ToUpperInvariant(value[0]) + value[1..];
        }

        private static int StableHash(string value)
        {
            var hash = 17;
            foreach (var c in value)
                hash = (hash * 31) + c;

            return Math.Abs(hash);
        }

        private static string EnsureUnique(string value, HashSet<string> seen)
        {
            var result = value;
            var suffix = 2;
            while (!seen.Add(result))
                result = $"{value}{suffix++}";

            return result;
        }
    }
}
