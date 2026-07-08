using CMS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CMS.Server.DB
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }
    }
}
