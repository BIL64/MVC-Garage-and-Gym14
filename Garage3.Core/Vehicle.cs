using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Garage3.Core
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Display(Name = "Regnummer"), StringLength(15, ErrorMessage = "Ange ett korrekt värde"), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public string RegNo { get; set; } = string.Empty;

        [Display(Name = "Fordonstyp")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Märke"), StringLength(25), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public string Brand { get; set; } = string.Empty;

        [Display(Name = "Modell"), StringLength(25), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public string Vmodel { get; set; } = string.Empty;

        [Display(Name = "Färg"), StringLength(15), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "Hjul"), Range(0, 50, ErrorMessage = "Ange ett korrekt nummer"), Required(ErrorMessage = "Detta fält är obligatoriskt")]
        public int Wheels { get; set; }

        [Display(Name = "P-ruta")]
        public string Place { get; set; } = string.Empty;

        [Display(Name = "Ankomsttid")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "P-tid")]
        public string ParkedTime { get; set; } = string.Empty;

        [Display(Name = "P")]
        public bool IsParked { get; set; }

        [Display(Name = "Intäkter")]
        public int InCome { get; set; }       

        [Display(Name = "Ägare")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "ID")]
        public string MemberNo { get; set; } = string.Empty;

        // Foreign keys
        [Display(Name = "Fordonstyp")]
        public int VtypeId { get; set; } // Typ av fordon int.

        [Display(Name = "ID")]
        public int MemberId { get; set; } // Samma id som ägaren.
    }
}
