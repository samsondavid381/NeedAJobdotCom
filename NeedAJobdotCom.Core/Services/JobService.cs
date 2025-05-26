using NeedAJobdotCom.Core.DTOs;
using NeedAJobdotCom.Core.Interfaces;

namespace NeedAJobdotCom.Core.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobAggregator _jobAggregator;
        
        public JobService(IJobRepository jobRepository, IJobAggregator jobAggregator)
        {
            _jobRepository = jobRepository;
            _jobAggregator = jobAggregator;
        }
        
        public async Task<JobListResponse> GetJobsAsync(JobFilterDto filter)
        {
            return await _jobRepository.GetJobsAsync(filter);
        }
        
        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _jobRepository.GetJobByIdAsync(id);
            if (job == null) return null;
            
            return new JobDto
            {
                Id = job.Id,
                Title = job.Title,
                Company = job.Company,
                CompanyLogo = job.CompanyLogo,
                Description = job.Description,
                Location = job.Location,
                IsRemote = job.IsRemote,
                Salary = job.Salary,
                Type = job.Type.ToString(),
                Category = job.Category,
                Tags = job.Tags,
                ApplyUrl = job.ApplyUrl,
                Source = job.Source,
                PostedDate = job.PostedDate,
                PostedAgo = CalculateTimeAgo(job.PostedDate)
            };
        }
        
        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _jobRepository.GetCategoriesAsync();
        }
        
        public async Task<List<string>> GetLocationsAsync()
        {
            return await _jobRepository.GetLocationsAsync();
        }
        
        public async Task RefreshJobsAsync()
        {
            await _jobAggregator.FetchJobsAsync("", 100);
        }
        
        private static string CalculateTimeAgo(DateTime postedDate)
        {
            var timeSpan = DateTime.UtcNow - postedDate;
            
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            
            return "Just now";
        }
    }
}