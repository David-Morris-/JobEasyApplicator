using System.ComponentModel.DataAnnotations;

namespace Jobs.EasyApply.Common.Models
{
    public enum JobProvider
    {
        LinkedIn,
        Indeed,
        Dice
    }

    public class AppliedJob
    {
        [Key]
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty; // Unique identifier from platform
        public string Url { get; set; } = string.Empty;
        public JobProvider Provider { get; set; } = JobProvider.LinkedIn; // Platform where job was applied
        public DateTime AppliedDate { get; set; }
        public bool Success { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
