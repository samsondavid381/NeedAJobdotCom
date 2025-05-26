using System.Text.Json;
using NeedAJobdotCom.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NeedAJobdotCom.Infrastructure.ExternalServices
{
    public interface IAdzunaService
    {
        Task<List<ExternalJobData>> FetchJobsAsync(string query = "software engineer", string location = "", int limit = 50);
    }
    
    public class AdzunaService : IAdzunaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdzunaService> _logger;
        private readonly string _apiId;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        
        public AdzunaService(HttpClient httpClient, IConfiguration configuration, ILogger<AdzunaService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _apiId = _configuration["Adzuna:ApiId"] ?? throw new ArgumentException("Adzuna API ID not configured");
            _apiKey = _configuration["Adzuna:ApiKey"] ?? throw new ArgumentException("Adzuna API Key not configured");
            _baseUrl = _configuration["Adzuna:BaseUrl"] ?? "https://api.adzuna.com/v1/api";
        }
        
        public async Task<List<ExternalJobData>> FetchJobsAsync(string query = "software engineer", string location = "", int limit = 50)
{
    try
    {
        // Build search query for entry-level positions
        var searchQuery = BuildEntryLevelQuery(query);
        var searchLocation = string.IsNullOrEmpty(location) ? "us" : location;
        
        var url = $"{_baseUrl}/jobs/us/search/1" +
                 $"?app_id={_apiId}" +
                 $"&app_key={_apiKey}" +
                 $"&results_per_page={Math.Min(limit, 50)}" +
                 $"&what={Uri.EscapeDataString(searchQuery)}" +
                 $"&content-type=application/json";
        
        _logger.LogInformation("Fetching jobs from Adzuna API: {Query}", searchQuery);
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        // Handle encoding issue by reading as bytes first
        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        var jsonContent = System.Text.Encoding.UTF8.GetString(responseBytes);
        
        _logger.LogDebug("Adzuna API Response (first 200 chars): {Response}", 
            jsonContent.Length > 200 ? jsonContent.Substring(0, 200) + "..." : jsonContent);
        
        var searchResponse = JsonSerializer.Deserialize<AdzunaResponse>(jsonContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        if (searchResponse?.Results == null)
        {
            _logger.LogWarning("No jobs returned from Adzuna API. Response was: {Response}", 
                jsonContent.Length > 500 ? jsonContent.Substring(0, 500) + "..." : jsonContent);
            return new List<ExternalJobData>();
        }
        
        _logger.LogInformation("Adzuna API returned {Count} jobs", searchResponse.Results.Count);
        
        return searchResponse.Results
            .Where(IsEntryLevelJob)
            .Take(limit)
            .Select(MapToExternalJobData)
            .ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching jobs from Adzuna API");
        return new List<ExternalJobData>();
    }
}

        private static string BuildEntryLevelQuery(string baseQuery)
        {
            return baseQuery;
            
}

private static bool IsEntryLevelJob(AdzunaJob job)
{
    var title = job.Title.ToLower();
    var description = job.Description.ToLower();
    
    // Hard exclude senior positions
    var seniorKeywords = new[]
    {
        "senior", "sr.", "sr ", "lead", "principal", "staff", 
        "manager", "director", "vp", "vice president", "chief", 
        "head of", "architect", "expert", "specialist",
        "5+ years", "6+ years", "7+ years", "8+ years", "9+ years", "10+ years",
        "experienced", "seasoned", "advanced"
    };
    
    // If title or description has senior keywords, exclude
    var isSenior = seniorKeywords.Any(keyword => 
        title.Contains(keyword) || description.Contains(keyword));
    
    if (isSenior)
    {
        return false; // Definitely exclude
    }
    
    // Also check for recent posting (last 30 days)
    var daysSincePosted = (DateTime.UtcNow - job.Created).TotalDays;
    if (daysSincePosted > 30)
    {
        return false; // Exclude old jobs
    }
    
    return true; // Include if not senior and not old
}
        
        private static ExternalJobData MapToExternalJobData(AdzunaJob job)
        {
            var salary = BuildSalaryString(job.SalaryMin, job.SalaryMax);
            var location = job.Location.DisplayName;
            var isRemote = location.ToLower().Contains("remote") || 
                          job.Description.ToLower().Contains("remote");
            
            return new ExternalJobData
            {
                ExternalId = job.Id,
                Title = job.Title,
                Company = job.Company.DisplayName,
                CompanyLogo = null, // Adzuna doesn't provide logos
                Description = CleanDescription(job.Description),
                Location = location,
                IsRemote = isRemote,
                Salary = salary,
                Type = MapJobType(job.ContractType),
                ApplyUrl = job.RedirectUrl,
                PostedDate = job.Created,
                Source = "Adzuna"
            };
        }
        
        private static string? BuildSalaryString(double? minSalary, double? maxSalary)
        {
            if (!minSalary.HasValue && !maxSalary.HasValue)
                return null;
                
            if (minSalary.HasValue && maxSalary.HasValue)
                return $"${minSalary:N0} - ${maxSalary:N0}";
            else if (minSalary.HasValue)
                return $"${minSalary:N0}+";
            else if (maxSalary.HasValue)
                return $"Up to ${maxSalary:N0}";
                
            return null;
        }
        
        private static string CleanDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "";
                
            // Remove HTML tags if present
            var cleaned = System.Text.RegularExpressions.Regex.Replace(description, "<[^>]*>", "");
            
            // Limit length
            if (cleaned.Length > 1000)
                cleaned = cleaned.Substring(0, 1000) + "...";
                
            return cleaned.Trim();
        }
        
        private static string MapJobType(string? contractType)
        {
            if (string.IsNullOrEmpty(contractType))
                return "FullTime";
                
            return contractType.ToLower() switch
            {
                "permanent" or "full_time" => "FullTime",
                "part_time" => "PartTime", 
                "contract" => "Contract",
                "temporary" => "Temporary",
                _ => "FullTime"
            };
        }
    }
}