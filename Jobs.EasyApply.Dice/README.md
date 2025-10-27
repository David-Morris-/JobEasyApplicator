# Jobs.EasyApply.Dice

A .NET console application for automated job application on Dice.com, specifically designed to filter and apply for Easy Apply jobs only.

## Features

- **Easy Apply Filtering**: Automatically filters job search results to only include Easy Apply jobs
- **Automated Job Search**: Searches Dice.com for jobs matching specified criteria
- **Smart Application Process**: Handles multi-step application forms with manual intervention when needed
- **Anti-Detection Measures**: Includes browser fingerprint randomization and stealth techniques
- **API Integration**: Tracks applications via REST API
- **Comprehensive Logging**: Detailed logging using Serilog

## Easy Apply Jobs

This application is specifically designed to work with Dice.com's Easy Apply jobs, which are:
- Jobs that can be applied to directly through Dice.com without leaving the platform
- Typically have simplified application processes
- Often pre-fill candidate information from Dice profile
- Usually have faster application turnaround

## Configuration

Update `appsettings.json` with your Dice.com credentials and job search preferences:

```json
{
  "Credentials": {
    "Email": "your-dice-email@example.com",
    "Password": "your-dice-password"
  },
  "JobSearchParams": {
    "Title": ".NET Developer",
    "Location": "Remote"
  }
}
```

## Usage

### Command Line
```bash
# Use default settings from appsettings.json
dotnet run

# Override job title and location
dotnet run "Software Engineer" "New York"
```

### Programmatic
```csharp
using Jobs.EasyApply.Dice.Services;

var scraper = new JobScraper("Software Engineer", "Remote", "email@example.com", "password");
var jobs = scraper.SearchJobs(); // Only returns Easy Apply jobs

foreach (var job in jobs)
{
    bool success = scraper.ApplyForJob(job);
    // Application result tracked via API
}
```

## How It Works

1. **Job Search**: Navigates to Dice.com with Easy Apply filter (`applyType=easy_apply`)
2. **Easy Apply Detection**: Scans job cards for Easy Apply indicators (badges, text, CSS classes)
3. **Application Process**: For each Easy Apply job:
   - Clicks Apply button
   - Handles multi-step forms
   - Prompts for manual input if additional questions require completion
   - Submits application
   - Tracks result via API

## Easy Apply Indicators

The application looks for these indicators to identify Easy Apply jobs:
- `data-cy="easy-apply-badge"` or similar data attributes
- CSS classes containing "easy-apply", "quick-apply", "instant-apply"
- Text content containing "Easy Apply", "Quick Apply", "Instant Apply"
- Apply buttons that don't redirect to external sites

## API Integration

Applications are tracked via HTTP API calls to:
- `GET /api/jobs/test-connection` - Test API connectivity
- `GET /api/jobs/check/{jobId}` - Check if job was previously applied to
- `POST /api/jobs` - Record job application with details

## Dependencies

- .NET 9.0
- Selenium WebDriver
- ChromeDriver
- Serilog
- Microsoft.Extensions.Configuration

## Building

```bash
# Build Dice project only
dotnet build Jobs.EasyApply.Dice/Jobs.EasyApply.Dice.csproj

# Build entire solution
dotnet build Jobs.EasyApply.sln
```

## Notes

- Ensure Chrome browser is installed
- Configure Dice.com credentials in `appsettings.json`
- API server must be running on `http://localhost:5070`
- Manual intervention may be required for complex application questions
- Application respects rate limits and includes delays to avoid being blocked

## Troubleshooting

- **No jobs found**: Check if Easy Apply filter is working or if job title/location has results
- **Application fails**: Verify Dice.com credentials and check browser console for errors
- **API connection fails**: Ensure API server is running and accessible
- **Chrome driver issues**: Update ChromeDriver version to match installed Chrome browser
