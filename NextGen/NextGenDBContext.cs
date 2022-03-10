using APSIM.Builds.Migrator.NextGen.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace APSIM.Builds.Migrator.NextGen
{
    /// <summary>
    /// DB context for the APSIM builds database.
    /// </summary>
    public class NextGenDBContext : DbContext
    {
        /// <summary>
        /// Available upgrades/versions of apsim.
        /// </summary>
        public DbSet<Upgrade> Upgrades => Set<Upgrade>();

        /// <summary>
        /// Create a new <see cref="NextGenDBContext"/>.
        /// </summary>
        /// <param name="options">DB context builder options.</param>
        public NextGenDBContext(DbContextOptions options) : base(options)
        {
        }
    }
}
