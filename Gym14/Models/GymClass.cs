using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;

namespace Gym14.Models
{
    public class GymClass
    {
        public int Id { get; set; }

        [Display(Name = "Sort of...")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Start time")]
        public DateTime StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime EndTime { get { return StartTime + Duration; } }

        public string Description { get; set; } = string.Empty;        

        public ICollection<ApplicationUserGymClass> AttendingMembers { get; set; } = new List<ApplicationUserGymClass>();
    }
}
