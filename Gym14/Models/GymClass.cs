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

        public string Start_Time { get { return StartTime.ToString()[..16]; } }

        public TimeSpan Duration { get; set; }

        public string DurString { get { return Duration.ToString()[..5]; } }

        [Display(Name = "End time")]
        public DateTime EndTime { get { return StartTime + Duration; } }

        public string End_Time { get { return EndTime.ToString()[11..16]; } }

        public string Description { get; set; } = string.Empty;        

        //public ICollection<ApplicationUserGymClass> AttendingMembers { get; set; } = new List<ApplicationUserGymClass>();
    }
}
