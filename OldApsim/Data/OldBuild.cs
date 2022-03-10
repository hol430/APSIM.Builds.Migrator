using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace APSIM.Builds.Migrator.OldApsim.Data
{
    [Table("Classic")]
    public class OldBuild
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? PatchFileName { get; set; }
        public string? Description { get; set; }
        public int BugID { get; set; }
        public string? Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public int? NumDiffs { get; set; }
        public int? RevisionNumber { get; set; }
        public int DoCommit { get; set; }
        public DateTime? UploadTime { get; set; }
        [Column("LinuxNumDiffs")]
        public int? LinuxNumDiffs { get; set; }
        [Column("linuxStatus")]
        public string? LinuxStatus { get; set; }
        public int JenkinsId { get; set; }
        public int? PullRequestId { get; set; }
    }
}
