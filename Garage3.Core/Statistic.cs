using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garage3.Core
{
    public class Statistic
    {
        public static int Bilar { get; set; }
        public const string nBilar = "antalet bilar";

        public static int Mcyklar { get; set; }
        public const string nMcyklar = "-motorcyklar";

        public static int Lbilar { get; set; }
        public const string nLbilar = "-lastbilar";

        public static int Bussar { get; set; }
        public const string nBussar = "-bussar";

        public static int Traktorer { get; set; }
        public const string nTraktorer = "-traktorer";

        public static int Emaskiner { get; set; }
        public const string nEmaskiner = "-entreprenadmaskiner";

        public static int Hjul { get; set; }
        public const string nHjul = "-hjul";

        public static float InCome { get; set; }
        public const string nInCome = "momentana intäkter";
    }
}
