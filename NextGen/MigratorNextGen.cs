using System;
using System.Collections.Generic;
using System.Linq;
using APSIM.Builds.Migrator.NextGen.Data;
using Microsoft.EntityFrameworkCore;

namespace APSIM.Builds.Migrator.NextGen
{
    /// <summary>
    /// Data migrator for the apsim next gen builds DB.
    /// </summary>
    internal class MigratorNextGen
    {
        /// <summary>
        /// Name of an environment variable which contains a connection string to
        /// the apsimdev builds classic DB.
        /// </summary>
        private const string oldConnectionStringVariable = "CONN_STR_NG_OLD";

        /// <summary>
        /// Name of an environment variable which contains a connection string to
        /// the new builds classic DB.
        /// </summary>
        private const string newConnectionStringVariable = "CONN_STR_NG_NEW";

        /// <summary>
        /// Migrate data from the old DB to the new one.
        /// </summary>
        public void Run()
        {
            IReadOnlyList<OldUpgrade> oldData = GetOldData();
            IReadOnlyList<Upgrade> newData = oldData.Select(Convert).ToList();
            WriteToNewDb(newData);
        }

        private Upgrade Convert(OldUpgrade old)
        {
            try
            {
                if (old.PullRequestId < 0)
                    throw new InvalidOperationException($"PR ID ({old.PullRequestId} is less than 0");

                uint issueNumber = GetIssueNumber(old);
                string issueTitle = GetIssueTitle(old, issueNumber);

                if (old.Version < 0)
                    throw new InvalidOperationException($"Version ({old.Version}) less than 0");
                
                return new Upgrade(
                    old.Id,
                    old.Date,
                    issueNumber,
                    (uint)old.PullRequestId,
                    issueTitle,
                    (uint)old.Version);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Unable to convert NextGen pull request {old.PullRequestId} ({old.IssueTitle})", error);
            }
        }

        private string GetIssueTitle(OldUpgrade old, uint issueNumber)
        {
            if (!string.IsNullOrEmpty(old.IssueTitle))
                return old.IssueTitle;
            GitHub github = new GitHub();
            if (issueNumber > int.MaxValue)
                throw new InvalidOperationException($"Issue number {issueNumber} too large");
            string issueTitle = github.GetIssueTitle(owner, repo, (int)issueNumber);
            if (string.IsNullOrEmpty(issueTitle))
                throw new InvalidOperationException($"Issue #{issueNumber} has no title");
            return issueTitle;
        }

        private const string owner = "APSIMInitiative";
        private const string repo = "ApsimX";

        private uint GetIssueNumber(OldUpgrade old)
        {
            if (old.IssueNumber > 0)
                return (uint)old.IssueNumber;
            GitHub github = new GitHub();
            int issueNumber = github.GetReferencedIssue(old.PullRequestId, owner, repo);
            if (issueNumber <= 0)
                throw new InvalidOperationException($"PR {old.PullRequestId} does not reference an issue");
            return (uint)issueNumber;
        }

        private void WriteToNewDb(IReadOnlyList<Upgrade> newData)
        {
            using (NextGenDBContext context = CreateNewDbContext())
            {
                context.Upgrades.AddRange(newData);
                context.SaveChanges();
            }
        }

        private IReadOnlyList<OldUpgrade> GetOldData()
        {
            using (NextGenOldDBContext context = CreateOldDbContext())
                return context.Upgrades.ToList();
        }

        private NextGenDBContext CreateNewDbContext()
        {
            string connectionString = EnvironmentVariable.Read(newConnectionStringVariable, "Connection string for new NG DB");
            var builder = new DbContextOptionsBuilder().UseLazyLoadingProxies().UseMySQL(connectionString);
            NextGenDBContext context = new NextGenDBContext(builder.Options);
            context.Database.EnsureCreated();
            return context;
        }

        private NextGenOldDBContext CreateOldDbContext()
        {
            string connectionString = EnvironmentVariable.Read(oldConnectionStringVariable, "Connection string for old NG DB");
            var builder = new DbContextOptionsBuilder().UseLazyLoadingProxies().UseSqlServer(connectionString);
            return new NextGenOldDBContext(builder.Options);
        }
    }
}
