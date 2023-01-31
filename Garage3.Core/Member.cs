using System.ComponentModel.DataAnnotations;

namespace Garage3.Core
{
    public class Member
    {
        public int Id { get; set; }

        [Display(Name = "ID"), StringLength(20), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public string MemberNo { get; set; } = string.Empty;

        [Display(Name = "Förnamn"), StringLength(30), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Efternamn"), StringLength(30), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Namn")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Personnummer (ååååmmdd-nnnn)"), StringLength(13), Required(ErrorMessage = "Detta fält är obligatoriskt"),
        RegularExpression(@"[1-9][0-9-]{12,14}$", ErrorMessage = "Inkorrekt personnummer")]
        public string PersonalNo { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>(); // De fordon som medlemmarna äger. Fylls i automatiskt...
    }
}