using System.Collections.Generic;

namespace Jobs.EasyApply.Common.Models
{
    public interface IJobScraper : IDisposable
    {
        Task<List<JobListing>> SearchJobsAsync();
        bool ApplyForJob(JobListing job);
    }
}
