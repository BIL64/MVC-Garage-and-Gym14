namespace Gym14.Models
{
    public static class Auxx
    {
        public static ICollection<GymClass> Gymlist { get; set; } = new List<GymClass>();

        public static ICollection<ApplicationUser> Userlist { get; set; } = new List<ApplicationUser>();

        public static ICollection<ApplicationUserGymClass> Usergymlist { get; set; } = new List<ApplicationUserGymClass>();

        public static string Book_Yes { get; set; } = string.Empty;

        public static string Book_No { get; set; } = string.Empty;

        public static string History { get; set; } = "History";

        public static char Cwidth { get; set; } = '4';

        public static byte Chist { get; set; }

        public static void Reset()
        {
            Book_Yes = string.Empty;
            Book_No = string.Empty;
        }

        public static void HReset()
        {
            History = "History";
            Cwidth = '4';
        }
    }
}
