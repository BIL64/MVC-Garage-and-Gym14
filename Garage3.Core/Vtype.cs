using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Garage3.Core
{
    public class Vtype
    {
        public int Id { get; set; }

        [Display(Name = "Fordonstyper")]
        public string Type { get; set; } = string.Empty;
    }
}
