using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garage3.Core
{
    public static class Auxiliary
    {
        public static string WarningReg { get; set; } = string.Empty; // Felmeddelande vid regnrkrock.

        public static string WarningName { get; set; } = string.Empty; // Felmeddelande vid namnkrock.

        public static string WarningMember { get; set; } = string.Empty; // Felmeddelande om ej medlem.

        public static string Thanks { get; set; } = string.Empty; // Tack för parkering.

        public static string Fullt { get; set; } = string.Empty; // Garaget är fullt.

        public static string Operation { get; set; } = string.Empty; // Operation genomförd.

        public static int Pricebase { get; set; } = 10; // Grundpriset. Config_bas.txt.

        public static int Pricehour { get; set; } = 20; // Priset per timme. Config_tim.txt.

        public static string[] Capacity { get; set; } = new string[20]; // Antalet lediga platser. Config_bas.txt.

        public static int Counter { get; set; } = 0; // Håller koll.

        public static bool Start { get; set; } = true; // Vid initiering då man inte vet hur många fordon det finns i tabellen.

        public static void Reset()
        {
            WarningReg = string.Empty;
            WarningName = string.Empty;
            WarningMember = string.Empty;
            Thanks = string.Empty;
            Fullt = string.Empty;
            Operation = string.Empty;
        }

        public static void ArrayReset(int cap) // Laddar den initierade arrayen med tomma poster.
        {
            for (int i = 0; i < cap; i++)
            {
                Auxiliary.Capacity[i] = string.Empty;
            }
            Auxiliary.Counter = 0;
        }
    }
}
