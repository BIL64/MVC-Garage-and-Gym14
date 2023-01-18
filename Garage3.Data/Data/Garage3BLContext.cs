using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Garage3.Core;

namespace Garage3BL.Data
{
    public class Garage3BLContext : DbContext
    {
        public Garage3BLContext (DbContextOptions<Garage3BLContext> options)
            : base(options)
        {
        }

        public DbSet<Garage3.Core.Member> Member { get; set; } = default!;

        public DbSet<Garage3.Core.Vehicle> Vehicle { get; set; } = default!;
    }
}
