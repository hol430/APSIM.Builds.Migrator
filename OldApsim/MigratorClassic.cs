using APSIM.Builds;
using APSIM.Builds.Data.OldApsim;
using APSIM.Builds.Migrator.OldApsim.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace APSIM.Builds.Migrator.OldApsim
{
    /// <summary>
    /// Data migrator for the apsim classic builds DB.
    /// </summary>
    internal class MigratorClassic
    {
        /// <summary>
        /// Name of an environment variable which contains a connection string to
        /// the apsimdev builds classic DB.
        /// </summary>
        private const string oldConnectionStringVariable = "CONN_STR_CLASSIC_OLD";

        /// <summary>
        /// Name of an environment variable which contains a connection string to
        /// the new builds classic DB.
        /// </summary>
        private const string newConnectionStringVariable = "CONN_STR_CLASSIC_NEW";

        /// <summary>
        /// Run the migrator, migrate the data from the old DB to the new one.
        /// </summary>
        public void Run()
        {
            IReadOnlyList<OldBuild> oldBuilds = GetOldBuilds();
            IReadOnlyList<Build> builds = oldBuilds.Select(Convert).ToList();
            using (OldApsimDbContext context = CreateNewDbContext())
            {
                context.Builds.AddRange(builds);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Get all builds from the old DB.
        /// </summary>
        private IReadOnlyList<OldBuild> GetOldBuilds()
        {
            using (OldClassicDbContext context = CreateOldDbContext())
                return context.Builds.ToList();
        }

        /// <summary>
        /// Convert an old build object to a new one.
        /// </summary>
        /// <param name="old">The old build.</param>
        private Build Convert(OldBuild old)
        {
            if (old.BugID < 0 && (old.PullRequestId == null || old.PullRequestId < 0))
                throw new InvalidOperationException($"Old build {old.Id} ({old.Description}) has no bug ID or pull request ID");
            GitHub github = new GitHub();
            uint bugId = old.BugID >= 0 ? (uint)old.BugID : (uint)github.GetReferencedIssue((int)old.PullRequestId!, "APSIMInitiative", "APSIMClassic");

            if (old.NumDiffs < 0)
                throw new InvalidOperationException($"Old build {old.Id} ({old.Description}) has {old.NumDiffs} diffs");
            if (old.RevisionNumber < 0)
                throw new InvalidOperationException($"Old build {old.Id} ({old.Description}) has revision number < 0: {old.RevisionNumber}");
            uint? jenkinsID = old.JenkinsId < 0 ? null : (uint?)old.JenkinsId;
            if (old.PullRequestId < 0)
                throw new InvalidOperationException($"Old build {old.Id} ({old.Description}) has pull request ID < 0: {old.PullRequestId}");
            if (old.UserName == null)
                throw new InvalidOperationException($"Old build {old.Id} ({old.Description}) has null username");
            if (old.Description == null)
                throw new InvalidOperationException($"Old build {old.Id} has null description");
            DateTime startTime = GetStartTime(old);
            return new Build()
            {
                Id = old.Id,
                Author = old.UserName,
                Title = old.Description,
                BugID = (uint)old.BugID,
                Pass = old.Status?.ToLower() == "pass",
                StartTime = startTime,
                FinishTime = old.FinishTime,
                NumDiffs = (uint?)old.NumDiffs,
                RevisionNumber = (uint?)old.RevisionNumber,
                JenkinsID = (uint?)old.JenkinsId,
                PullRequestID = (uint?)old.PullRequestId
            };
        }

        private DateTime GetStartTime(OldBuild old)
        {
            if (old.StartTime != null)
                return (DateTime)old.StartTime;
            if (old.UploadTime != null)
                return (DateTime)old.UploadTime;
            if (string.IsNullOrEmpty(old.PatchFileName))
                throw new InvalidOperationException($"Unable to parse start time for build {old.Id} ({old.Description})");

            return ParseDate(old.PatchFileName);
        }

        private DateTime ParseDate(string patchFileName)
        {
            MatchCollection matches = Regex.Matches(patchFileName, @"(\d{2}-\d{2}-\d{4})");
            if (!matches.Any())
                throw new InvalidOperationException($"Unable to parse date from patch file name {patchFileName}");
            return DateTime.ParseExact(matches[0].Value, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Create a DB context for reading from the old DB.
        /// </summary>
        private OldClassicDbContext CreateOldDbContext()
        {
            string connectionString = EnvironmentVariable.Read(oldConnectionStringVariable, "Connection string to apsimdev old apsim DB");
            var builder = new DbContextOptionsBuilder().UseLazyLoadingProxies().UseSqlServer(connectionString);
            return new OldClassicDbContext(builder.Options);
        }

        /// <summary>
        /// Create a DB context for writing to the new DB.
        /// </summary>
        private OldApsimDbContext CreateNewDbContext()
        {
            string connectionString = EnvironmentVariable.Read(newConnectionStringVariable, "Connection string to apsimdev old apsim DB");
            var builder = new DbContextOptionsBuilder().UseLazyLoadingProxies().UseMySQL(connectionString);
            OldApsimDbContext context = new OldApsimDbContext(builder.Options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
