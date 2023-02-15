using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Gym14.Models;

namespace Gym14.Data
{
    //public class ApplicationDbContext : IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GymClass> GymClass { get; set; } = default!;

        public DbSet<ApplicationUser> AppUser { get; set; } = default!;

        public DbSet<ApplicationUserGymClass> AppGymClass { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUserGymClass>().HasKey(t => new { t.ApplicationUserId, t.GymClassId });

            //modelBuilder.Entity<GymClass>().HasQueryFilter(g => g.StartTime > DateTime.UtcNow);
        }
    }
}