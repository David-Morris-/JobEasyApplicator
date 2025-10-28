namespace Jobs.EasyApply.Common.Models
{
    public interface IJobServiceFactory
    {
        IJobScraper CreateJobScraper(string jobTitle, string location, string email, string password, int maxJobsToApply = 50);
    }
}
