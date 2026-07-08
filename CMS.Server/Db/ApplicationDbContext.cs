using CMS.Server.Interfaces;
using CMS.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace CMS.Server.DB
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }
    }
}

