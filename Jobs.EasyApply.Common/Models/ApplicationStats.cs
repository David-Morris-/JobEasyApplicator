namespace Jobs.EasyApply.Common.Models
{
    public class ApplicationStats
    {
        public int TotalApplications { get; set; }
        public int SuccessfulApplications { get; set; }
        public int FailedApplications { get; set; }
        public double SuccessRate => TotalApplications > 0 ? (double)SuccessfulApplications / TotalApplications * 100 : 0;
    }
}
