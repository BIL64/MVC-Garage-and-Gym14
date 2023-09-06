namespace Gym14.Models
{
    public class ApplicationUserGymClass // Kopplingstabell.
    {
        public string ApplicationUserId { get; set; } = string.Empty;

        public int GymClassId { get; set; }

        public ApplicationUser? ApplicationUser { get; set; }

        public GymClass? GymClass { get; set;}
    }
}
