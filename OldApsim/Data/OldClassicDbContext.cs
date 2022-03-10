using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace APSIM.Builds.Migrator.OldApsim.Data
{
    /// <summary>
    /// DB context for the apsimdev schema of the old apsim builds DB.
    /// </summary>
    internal class OldClassicDbContext : DbContext
    {
        /// <summary>
        /// Builds of apsim.
        /// </summary>
        public DbSet<OldBuild> Builds => Set<OldBuild>();

        /// <summary>
        /// Create a new <see cref="OldClassicDbContext"/>.
        /// </summary>
        /// <param name="options">DB context builder options.</param>
        public OldClassicDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
