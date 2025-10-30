using Jobs.EasyApply.Common.Models;

namespace Jobs.EasyApply.Indeed.Services
{
    public class IndeedJobServiceFactory : IJobServiceFactory
    {
        public IJobScraper CreateJobScraper(string jobTitle, string location, string email, string password, int maxJobsToApply = 50)
        {
            return new JobScraper(jobTitle, location, email, password);
        }
    }
}
