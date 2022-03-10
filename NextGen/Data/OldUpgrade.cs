using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace APSIM.Builds.Migrator.NextGen.Data
{
    [Table("ApsimX")]
    public class OldUpgrade
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int PullRequestId { get; set; }
        public int IssueNumber { get; set; }
        public string IssueTitle { get; set; }
        public bool Released { get; set; }
        public int Version { get; set; }
    }
}
