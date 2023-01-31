using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gym14.Models
{
    public class ApplicationUser : IdentityUser
    {
        public override string Id { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public ICollection<ApplicationUserGymClass> AttendedClasses { get; set; } = new List<ApplicationUserGymClass>();        
    }
}
