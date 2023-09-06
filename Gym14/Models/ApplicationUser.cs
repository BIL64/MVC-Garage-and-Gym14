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

        [Display(Name = "Booking member")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "USMA")]
        public char Rtype { get; set; } = 'U';

        [Display(Name = "Arrived date")]
        public string Arrived { get; set; } = DateTime.Now.ToString().Substring(0, 10);

        //public ICollection<ApplicationUserGymClass> AttendedClasses { get; set; } = new List<ApplicationUserGymClass>();        
    }
}
