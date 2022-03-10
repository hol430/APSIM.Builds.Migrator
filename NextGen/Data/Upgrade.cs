using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace APSIM.Builds.Migrator.NextGen.Data
{
    /// <summary>
    /// An class encapsulating an APSIM Next Gen upgrade.
    /// </summary>
    [Table("ApsimX")]
    public class Upgrade
    {
        /// <summary>
        /// ID of the upgrade in the database.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Number/ID of the issue addressed by this upgrade.
        /// </summary>
        public uint IssueNumber { get; private set; }

        /// <summary>
        /// Number/ID of the pull request which generated this upgrade.
        /// </summary>
        public uint PullRequestNumber { get; private set; }

        /// <summary>
        /// Title of the issue addressed by this upgrade.
        /// </summary>
        public string IssueTitle { get; private set; }

        /// <summary>
        /// Release date of the upgrade.
        /// </summary>
        public DateTime ReleaseDate { get; private set; }

        /// <summary>
        /// Revision number of the upgrade.
        /// </summary>
        public uint Revision { get; private set; }

        /// <summary>
        /// Create an <see cref="Upgrade" /> instance.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for use by the entity framework, and
        /// should not be called directly from user code.
        /// </remarks>
        /// <param name="id">ID of the upgrade in the database.
        /// <param name="releaseDate">Release date of the upgrade.</param>
        /// <param name="issueNumber">Number/ID of the issue addressed by this upgrade.</param>
        /// <param name="pullRequestNumber">Number/ID of the pull request which generated this upgrade.</param>
        /// <param name="issueTitle">Upgrade title.</param>
        /// <param name="revision">Revision number of the upgrade.</param>
        public Upgrade(int id, DateTime releaseDate, uint issueNumber, uint pullRequestNumber, string issueTitle, uint revision)
            : this(issueNumber, pullRequestNumber, issueTitle, revision)
        {
            Id = id;
            ReleaseDate = releaseDate;
        }

        /// <summary>
        /// Create an <see cref="Upgrade" /> instance.
        /// </summary>
        /// <param name="issueNumber">Number/ID of the issue addressed by this upgrade.</param>
        /// <param name="pullRequestNumber">Number/ID of the pull request which generated this upgrade.</param>
        /// <param name="issueTitle">Upgrade title.</param>
        /// <param name="revision">Revision number of the upgrade.</param>
        public Upgrade(uint issueNumber, uint pullRequestNumber, string issueTitle, uint revision)
        {
            ReleaseDate = DateTime.Now;
            IssueNumber = issueNumber;
            PullRequestNumber = pullRequestNumber;
            IssueTitle = issueTitle;
            Revision = revision;
        }
    }
}
