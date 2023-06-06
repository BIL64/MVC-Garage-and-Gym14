using Bogus;
using Gym14.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Gym14.Data
{
    public class SeedDataMax
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

            var gympass = GenerateGympass(10); // Laddar tabellen med gympass enligt boguspricipen.
            db.AddRange(gympass);
            await db.SaveChangesAsync();

            var connections = await GenerateUsers(gympass, 1, 4, Title_user, Password_user); // Kopplar ett slumptal stycken användare till gympassen enligt bogusprincipen.
            db.AddRange(connections); // Kopplingstabellen knyter ihop medlemmarna med gympassen.
            await db.SaveChangesAsync();

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

        private static async Task<List<ApplicationUserGymClass>> GenerateUsers(IEnumerable<GymClass> gympass, int min, int max, string title, string pass)
        {
            var connections = new List<ApplicationUserGymClass>();
            var rnd = new Random();

            foreach (var gpass in gympass)
            {
                var members = GenerateMembers(rnd.Next(min, max));

                foreach (var member in members)
                {
                    await userManager.CreateAsync(member, pass);
                    await userManager.AddToRoleAsync(member, title);

                    var connect = new ApplicationUserGymClass
                    {
                        ApplicationUserId = member.Id,
                        GymClassId = gpass.Id
                    };
                    connections.Add(connect);
                }
            }
            return connections;
        }

        private static List<ApplicationUser> GenerateMembers(int nrOfMembers)
        {
            var members = new List<ApplicationUser>();
            var faker = new Faker("sv"); // Svenska namn.

            for (int i = 0; i < nrOfMembers; i++)
            {
                var fname = faker.Name.FirstName();
                var lname = faker.Name.LastName();
                var email = faker.Internet.Email(fname, lname, "gym14.se");
                var phoneNumber = faker.Phone.PhoneNumberFormat();

                var member = new ApplicationUser
                {
                    FirstName = fname,
                    LastName = lname,
                    Rtype = 'S',
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = true
                };

                members.Add(member);
            }
            return members;
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
