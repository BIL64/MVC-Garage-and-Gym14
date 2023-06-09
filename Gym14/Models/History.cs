using System.ComponentModel.DataAnnotations;

namespace Gym14.Models
{
    public class History
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Display(Name = "USMA")]
        public char Rtype { get; set; } = 'M';

        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone number")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Arrived date")]
        public string Date { get; set; } = string.Empty;
    }
}
