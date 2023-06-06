using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;

namespace Gym14.Models
{
    public class History
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public char Rtype { get; set; } = 'M';

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;
    }
}
