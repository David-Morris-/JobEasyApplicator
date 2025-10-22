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
        public bool PreviouslyApplied { get; set; } = false;
    }
}
