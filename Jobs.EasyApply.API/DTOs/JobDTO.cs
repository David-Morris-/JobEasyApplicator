namespace Jobs.EasyApply.API.DTOs
{
    public class JobDTO
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty; // Platform where job was applied
        public DateTime AppliedDate { get; set; }
        public bool Success { get; set; }
    }
}
