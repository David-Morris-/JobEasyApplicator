using System.Linq.Expressions;
using Jobs.EasyApply.Common.Models;

namespace Jobs.EasyApply.Infrastructure.Repositories.Specifications
{
    /// <summary>
    /// Specification for filtering job applications by company name
    /// </summary>
    public class JobApplicationByCompanySpecification : BaseSpecification<AppliedJob>
    {
        public JobApplicationByCompanySpecification(string companyName)
            : base(job => job.Company.ToLower().Contains(companyName.ToLower()))
        {
            AddOrderByDescending(job => job.AppliedDate);
        }
    }

    /// <summary>
    /// Specification for filtering job applications by date range
    /// </summary>
    public class JobApplicationByDateRangeSpecification : BaseSpecification<AppliedJob>
    {
        public JobApplicationByDateRangeSpecification(DateTime startDate, DateTime endDate)
            : base(job => job.AppliedDate >= startDate && job.AppliedDate <= endDate)
        {
            AddOrderByDescending(job => job.AppliedDate);
        }
    }

    /// <summary>
    /// Specification for filtering successful job applications
    /// </summary>
    public class SuccessfulJobApplicationsSpecification : BaseSpecification<AppliedJob>
    {
        public SuccessfulJobApplicationsSpecification()
            : base(job => job.Success)
        {
            AddOrderByDescending(job => job.AppliedDate);
        }
    }

    /// <summary>
    /// Specification for filtering failed job applications
    /// </summary>
    public class FailedJobApplicationsSpecification : BaseSpecification<AppliedJob>
    {
        public FailedJobApplicationsSpecification()
            : base(job => !job.Success)
        {
            AddOrderByDescending(job => job.AppliedDate);
        }
    }

    /// <summary>
    /// Specification for filtering job applications by job ID
    /// </summary>
    public class JobApplicationByJobIdSpecification : BaseSpecification<AppliedJob>
    {
        public JobApplicationByJobIdSpecification(string jobId)
            : base(job => job.JobId == jobId)
        {
        }
    }

    /// <summary>
    /// Specification for getting recent job applications with pagination
    /// </summary>
    public class RecentJobApplicationsSpecification : BaseSpecification<AppliedJob>
    {
        public RecentJobApplicationsSpecification(int skip, int take)
            : base()
        {
            AddOrderByDescending(job => job.AppliedDate);
            ApplyPaging(skip, take);
        }
    }
}
