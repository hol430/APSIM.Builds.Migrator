using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace APSIM.Builds.Data.OldApsim
{
    [Table("BuildsClassic")]
    public class Build
    {
        /// <summary>
        /// Build ID.
        /// </summary>
        public int Id { get; set; }

#nullable disable
        /// <summary>
        /// Patch/build author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Build title/description.
        /// </summary>
        public string Title { get; set; }
#nullable enable

        /// <summary>
        /// Number/ID of the bug addressed by this build.
        /// </summary>
        /// <remarks>
        /// For older builds, this will be the ID of a bug on
        /// the old bug tracker website. For newer builds, this
        /// will be a github issue number.
        /// </remarks>
        public uint BugID { get; set; }

        /// <summary>
        /// Did the build pass?
        /// </summary>
        public bool Pass { get; set;}

        /// <summary>
        /// Start time of the build.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Finish time of the build. This will be null if the build
        /// has not finished running.
        /// </summary>
        public DateTime? FinishTime { get; set; }

        /// <summary>
        /// Number of diffs in this build.
        /// This will be null if the build has not finished running.
        /// </summary>
        public uint? NumDiffs { get; set; }

        /// <summary>
        /// Revision number of this build. This will be null if the build
        /// has not finished running.
        /// </summary>
        public uint? RevisionNumber { get; set; }

        /// <summary>
        /// If this job was built on jenkins, this will be the ID
        /// of the jenkins job. This will be null for builds which
        /// ran on bob.
        /// </summary>
        /// <remarks>
        /// This is used to provide a link to the job on the builds page.
        /// </remarks>
        public uint? JenkinsID { get; set; }

        /// <summary>
        /// Number/ID of the pull request which triggered this build.
        /// This will be null for builds which ran on bob.
        /// </summary>
        public uint? PullRequestID { get; set; }
    }
}
