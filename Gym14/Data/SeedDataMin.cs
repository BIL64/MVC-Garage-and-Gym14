using Bogus;
using Gym14.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Gym14.Data
{
    public class SeedDataMin
    {
        private static RoleManager<IdentityRole> roleManager = default!;
        private static UserManager<ApplicationUser> userManager = default!;

        public static async Task InitAsync(ApplicationDbContext db, IServiceProvider services)
        {
            if (await db.Gclass.AnyAsync()) return; // Hoppar ut om tabellen inte är tom.

            roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            ArgumentNullException.ThrowIfNull(roleManager);

            userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            ArgumentNullException.ThrowIfNull(userManager);

            string Title_user = "Members";
            string Title_admin = "Administrators";
            string Password_user = $"Pass{Title_user}@Word00!";
            string Password_admin = $"Pass{Title_admin}@Word00!";
            string Fname_admin = "John";
            string Lname_admin = "Doe";
            string Email_admin = $"{Fname_admin}@{Lname_admin}.se";
            string Phone_admin = "0708-12233";

            await roleManager.CreateAsync(new IdentityRole { Name = Title_user }); // Skapar en roll-titel för användare.

            await roleManager.CreateAsync(new IdentityRole { Name = Title_admin }); // Skapar en roll-titel för administratörer.

            var gympass = GenerateGympass(1); // Laddar tabellen med ett gympass enligt boguspricipen.
            db.AddRange(gympass);
            await db.SaveChangesAsync();
;
            await GenerateUser("Test", "Testsson", "0709-45566", Title_user, "test@testsson.se", Password_user); // Skapar en fördefinerad medlem.
            await GenerateAdmin(Fname_admin, Lname_admin, Phone_admin, Title_admin, Email_admin, Password_admin); // Skapar en fördefinerad administratör.
            await db.SaveChangesAsync();
        }

        private static IEnumerable<GymClass> GenerateGympass(int nrOfGympass)
        {
            var gympass = new List<GymClass>();
            var faker = new Faker("sv");
            var rnd = new Random();
            int[] minutes = new int[] { 60, 90, 120, 150, 180, 210, 240 }; // Olika tider om +30 minuter som slumpas fram.

            for (int i = 0; i < nrOfGympass; i++)
            {
                var pass = new GymClass
                {
                    Name = faker.Music.Genre(),
                    StartTime = faker.Date.Future(),
                    Duration = TimeSpan.FromMinutes(minutes[rnd.Next(0, 6)]),
                    Description = faker.Hacker.Adjective(),
                };

                gympass.Add(pass);
            }
            return gympass;
        }

        private static async Task GenerateUser(string fname, string lname, string phone, string title, string email, string pass)
        {
            var admin = new ApplicationUser
            {
                FirstName = fname,
                LastName = lname,
                Rtype = 'S',
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true
            };

            await userManager.CreateAsync(admin, pass);
            await userManager.AddToRoleAsync(admin, title);
        }

        private static async Task GenerateAdmin(string fname, string lname, string phone, string title, string email, string pass)
        {
            var admin = new ApplicationUser
            {
                FirstName = fname,
                LastName = lname,
                Rtype = 'A',
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true
            };

            await userManager.CreateAsync(admin, pass);
            await userManager.AddToRoleAsync(admin, title);
        }
    }
}
