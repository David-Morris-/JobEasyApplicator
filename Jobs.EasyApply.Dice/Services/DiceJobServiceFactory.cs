using Jobs.EasyApply.Common.Models;

namespace Jobs.EasyApply.Dice.Services
{
    public class DiceJobServiceFactory : IJobServiceFactory
    {
        public IJobScraper CreateJobScraper(string jobTitle, string location, string email, string password, int maxJobsToApply = 50)
        {
            return new JobScraper(jobTitle, location, email, password);
        }
    }
}
