namespace Jobs.EasyApply.Common.Models
{
    public class AppSettings
    {
        public Credentials Credentials { get; set; } = new Credentials();
        public JobSearchParams JobSearchParams { get; set; } = new JobSearchParams();
    }

    public class Credentials
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class JobSearchParams
    {
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
