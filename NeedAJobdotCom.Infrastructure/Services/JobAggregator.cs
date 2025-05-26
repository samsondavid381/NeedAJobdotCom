using NeedAJobdotCom.Core.Entities;
using NeedAJobdotCom.Core.Interfaces;
using NeedAJobdotCom.Infrastructure.ExternalServices;
using NeedAJobdotCom.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Logging;

namespace NeedAJobdotCom.Infrastructure.Services
{
    public class JobAggregator : IJobAggregator
    {
        private readonly IAdzunaService _adzunaService;
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<JobAggregator> _logger;
        
        // Define search queries for each category
        private readonly Dictionary<string, string[]> _categoryQueries = new()
        {
            { "software-engineer", new[] { "software engineer", "web developer", "software developer", "full stack developer" } },
            { "data", new[] { "data analyst", "data scientist", "business analyst", "data engineer" } },
            { "design", new[] { "ux designer", "ui designer", "graphic designer", "product designer" } },
            { "cybersecurity", new[] { "cybersecurity analyst", "security analyst", "information security" } },
            { "ai-ml", new[] { "machine learning engineer", "ai engineer", "ml engineer" } },
            { "product", new[] { "product manager", "product analyst", "product coordinator" } },
            { "devops", new[] { "devops engineer", "site reliability engineer", "cloud engineer" } },
            { "qa", new[] { "qa engineer", "test engineer", "software tester", "quality assurance" } }
        };
        
        public JobAggregator(IAdzunaService adzunaService, IJobRepository jobRepository, ILogger<JobAggregator> logger)
        {
            _adzunaService = adzunaService;
            _jobRepository = jobRepository;
            _logger = logger;
        }
        
        public async Task<List<Job>> FetchJobsAsync(string category = "", int limit = 100)
        {
            var allJobs = new List<Job>();
            
            if (string.IsNullOrEmpty(category))
            {
                // Fetch from all categories
                return await RefreshAllCategoriesAsync() > 0 ? allJobs : new List<Job>();
            }
            else
            {
                // Fetch from specific category
                var jobCount = await RefreshCategoryAsync(category, limit);
                return jobCount > 0 ? allJobs : new List<Job>();
            }
        }
        
        public async Task<int> RefreshAllCategoriesAsync()
        {
            int totalJobsAdded = 0;
            
            foreach (var category in _categoryQueries.Keys)
            {
                try
                {
                    var jobsAdded = await RefreshCategoryAsync(category, 25); // Limit per category to stay within API limits
                    totalJobsAdded += jobsAdded;
                    
                    _logger.LogInformation("Added {JobCount} jobs for category {Category}", jobsAdded, category);
                    
                    // Rate limiting - be respectful to API
                    await Task.Delay(2000); // 2 second delay between category requests
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching jobs for category: {Category}", category);
                }
            }
            
            _logger.LogInformation("Total jobs added across all categories: {TotalCount}", totalJobsAdded);
            return totalJobsAdded;
        }
        
        public async Task<int> RefreshCategoryAsync(string category, int limit = 50)
        {
            if (!_categoryQueries.ContainsKey(category))
            {
                _logger.LogWarning("Unknown category: {Category}", category);
                return 0;
            }
            
            var queries = _categoryQueries[category];
            var totalJobsAdded = 0;
            var jobsPerQuery = Math.Max(1, limit / queries.Length);
            
            foreach (var query in queries)
            {
                try
                {
                    _logger.LogInformation("Fetching jobs for query: {Query} (category: {Category})", query, category);
                    
                    var externalJobs = await _adzunaService.FetchJobsAsync(query, "", jobsPerQuery);
                    var jobsAdded = await ProcessExternalJobs(externalJobs, category);
                    
                    totalJobsAdded += jobsAdded;
                    
                    _logger.LogInformation("Added {JobCount} jobs for query: {Query}", jobsAdded, query);
                    
                    // Rate limiting between queries
                    await Task.Delay(1000); // 1 second delay between queries
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching jobs for query: {Query}", query);
                }
            }
            
            return totalJobsAdded;
        }
        
        private async Task<int> ProcessExternalJobs(List<ExternalJobData> externalJobs, string category)
        {
            var jobsAdded = 0;
            
            foreach (var externalJob in externalJobs)
            {
                try
                {
                    // Check if job already exists to prevent duplicates
                    if (await _jobRepository.JobExistsAsync(externalJob.ExternalId, externalJob.Source))
                    {
                        _logger.LogDebug("Job already exists: {ExternalId} from {Source}", externalJob.ExternalId, externalJob.Source);
                        continue;
                    }
                    
                    var job = MapExternalJobToEntity(externalJob, category);
                    await _jobRepository.CreateJobAsync(job);
                    jobsAdded++;
                    
                    _logger.LogDebug("Created new job: {Title} at {Company}", job.Title, job.Company);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing external job: {ExternalId}", externalJob.ExternalId);
                }
            }
            
            return jobsAdded;
        }
        
        private static Job MapExternalJobToEntity(ExternalJobData externalJob, string category)
        {
            return new Job
            {
                Title = externalJob.Title,
                Company = externalJob.Company,
                CompanyLogo = externalJob.CompanyLogo,
                Description = externalJob.Description,
                Location = externalJob.Location,
                IsRemote = externalJob.IsRemote,
                Salary = externalJob.Salary,
                Type = MapStringToJobType(externalJob.Type),
                Category = category,
                Tags = GenerateTags(externalJob, category),
                ApplyUrl = externalJob.ApplyUrl,
                Source = externalJob.Source,
                ExternalId = externalJob.ExternalId,
                PostedDate = externalJob.PostedDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        
        private static JobType MapStringToJobType(string type)
        {
            return type switch
            {
                "FullTime" => JobType.FullTime,
                "PartTime" => JobType.PartTime,
                "Contract" => JobType.Contract,
                "Temporary" => JobType.Temporary,
                "Internship" => JobType.Internship,
                _ => JobType.FullTime
            };
        }
        
        private static List<string> GenerateTags(ExternalJobData externalJob, string category)
{
    var tags = new List<string>();
    var title = externalJob.Title.ToLower();
    var description = externalJob.Description.ToLower();
    
    // Category tags
    switch (category)
    {
        case "software-engineer":
            tags.Add("Software Engineer");
            if (title.Contains("full stack")) tags.Add("Full Stack");
            if (title.Contains("frontend") || title.Contains("front-end")) tags.Add("Frontend");
            if (title.Contains("backend") || title.Contains("back-end")) tags.Add("Backend");
            break;
        case "data":
            tags.Add("Data Science");
            break;
        // ... other categories
    }
    
    // Experience level (smart detection)
    var entryKeywords = new[] { "junior", "entry", "graduate", "new grad", "associate", "intern", "trainee" };
    var midKeywords = new[] { "mid", "intermediate", "2-3 years", "3-5 years" };
    var seniorKeywords = new[] { "senior", "lead", "principal", "5+ years", "experienced" };
    
    if (entryKeywords.Any(k => title.Contains(k) || description.Contains(k)))
        tags.Add("Entry Level");
    else if (midKeywords.Any(k => title.Contains(k) || description.Contains(k)))
        tags.Add("Mid Level");
    else if (seniorKeywords.Any(k => title.Contains(k) || description.Contains(k)))
        tags.Add("Senior Level");
    else
        tags.Add("Entry Level"); // Default assumption
    
    // Location tags
    if (externalJob.IsRemote)
        tags.Add("Remote");
    else
        tags.Add("On-site");
    
    return tags;
}
    }
}