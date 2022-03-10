using APSIM.Builds.Migrator.NextGen.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace APSIM.Builds.Migrator.NextGen
{
    /// <summary>
    /// DB context for the APSIM builds database.
    /// </summary>
    public class NextGenOldDBContext : DbContext
    {
        /// <summary>
        /// Available upgrades/versions of apsim.
        /// </summary>
        public DbSet<OldUpgrade> Upgrades => Set<OldUpgrade>();

        /// <summary>
        /// Create a new <see cref="NextGenOldDBContext"/>.
        /// </summary>
        /// <param name="options">DB context builder options.</param>
        public NextGenOldDBContext(DbContextOptions options) : base(options)
        {
        }
    }
}
