using System;

namespace MagiQL.Framework.Model.Response
{
    public class ReportStatus
    {
        public long Id { get; set; }
        public int? OrganizationId { get; set; }
        public int? CreatedByUserId { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }

        public TimeSpan Duration
        {
            get { return (DateCompleted ?? DateTime.Now) - DateCreated; }
        }

        public string Platform { get; set; }

        public string StatusMessage { get; set; }
        public int ProgressPercentage { get; set; }

        public string MachineName { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; } 
    }
}