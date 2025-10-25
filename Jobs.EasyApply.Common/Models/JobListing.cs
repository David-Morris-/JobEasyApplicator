using System.ComponentModel.DataAnnotations;

namespace Jobs.EasyApply.Common.Models
{
    public class JobListing
    {
        [Key]
        public string JobId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public JobProvider Provider { get; set; } = JobProvider.LinkedIn; // Platform where job was found
        public bool PreviouslyApplied { get; set; } = false;
    }
}
