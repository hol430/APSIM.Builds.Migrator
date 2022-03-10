using APSIM.Builds.Migrator.OldApsim;
using APSIM.Builds.Migrator.NextGen;
using System;

namespace APSIM.Builds.Migrator
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Migrating old apsim data...");
            // MigratorClassic migrator = new MigratorClassic();
            // migrator.Run();
            Console.WriteLine("Done.");

            Console.WriteLine("Migrating next gen data...");
            MigratorNextGen nextGenMigrator = new MigratorNextGen();
            nextGenMigrator.Run();
            Console.WriteLine("Done.");

            return 0;
        }
    }
}
