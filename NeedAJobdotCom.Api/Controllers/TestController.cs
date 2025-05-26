using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeedAJobdotCom.Infrastructure.Data;
using NeedAJobdotCom.Core.Entities;
using NeedAJobdotCom.Infrastructure.ExternalServices;

namespace NeedAJobdotCom.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly JobBoardContext _context;
        private readonly IConfiguration _configuration;
        
        public TestController(JobBoardContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        [HttpGet("database-test")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                var jobCount = await _context.Jobs.CountAsync();
                
                return Ok(new { 
                    CanConnect = canConnect, 
                    JobCount = jobCount,
                    DatabaseName = _context.Database.GetDbConnection().Database,
                    Message = "Database connection successful!" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message 
                });
            }
        }
        
        [HttpPost("add-test-job")]
        public async Task<IActionResult> AddTestJob()
        {
            try
            {
                var testJob = new Job
                {
                    Title = "Test Software Engineer Position",
                    Company = "Test Company Inc",
                    Description = "This is a test job created to verify database functionality.",
                    Location = "Remote",
                    IsRemote = true,
                    Type = JobType.FullTime,
                    Category = "software-engineer",
                    Tags = new List<string> { "Software Engineer", "Remote", "Entry Level" },
                    Source = "Manual Test",
                    PostedDate = DateTime.UtcNow
                };
                
                _context.Jobs.Add(testJob);
                await _context.SaveChangesAsync();
                
                return Ok(new { 
                    Message = "Test job created successfully!",
                    JobId = testJob.Id,
                    JobTitle = testJob.Title
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message 
                });
            }
        }
        
        [HttpGet("get-test-jobs")]
        public async Task<IActionResult> GetTestJobs()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Where(j => j.IsActive)
                    .OrderByDescending(j => j.PostedDate)
                    .Take(10)
                    .Select(j => new {
                        j.Id,
                        j.Title,
                        j.Company,
                        j.Location,
                        j.Type,
                        j.Category,
                        j.Tags,
                        j.PostedDate,
                        j.Source
                    })
                    .ToListAsync();
                
                return Ok(new {
                    JobCount = jobs.Count,
                    Jobs = jobs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message 
                });
            }
        }
        
        [HttpGet("test-adzuna-simple")]
        public async Task<IActionResult> TestAdzunaSimple()
        {
            try
            {
                var apiId = _configuration["Adzuna:ApiId"];
                var apiKey = _configuration["Adzuna:ApiKey"];
                
                // Simple test with just "software engineer" (no entry-level filtering)
                var url = $"https://api.adzuna.com/v1/api/jobs/us/search/1" +
                         $"?app_id={apiId}" +
                         $"&app_key={apiKey}" +
                         $"&results_per_page=5" +
                         $"&what=software%20engineer";
                
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);
                
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                var jsonContent = System.Text.Encoding.UTF8.GetString(responseBytes);
                
                return Ok(new { 
                    StatusCode = (int)response.StatusCode,
                    Url = url,
                    ResponseLength = jsonContent.Length,
                    RawResponse = jsonContent.Length > 2000 ? jsonContent.Substring(0, 2000) + "..." : jsonContent
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        
        [HttpGet("test-adzuna")]
        public async Task<IActionResult> TestAdzuna()
        {
            try
            {
                var adzunaService = HttpContext.RequestServices.GetRequiredService<IAdzunaService>();
                var jobs = await adzunaService.FetchJobsAsync("software engineer", "", 5);
                
                return Ok(new { 
                    Message = "Adzuna API test successful",
                    JobCount = jobs.Count,
                    Jobs = jobs.Take(3).Select(j => new { 
                        j.Title, 
                        j.Company, 
                        j.Location, 
                        j.ExternalId,
                        j.Source 
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
        
        [HttpGet("show-all-jobs")]
        public async Task<IActionResult> ShowAllJobs()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Select(j => new { 
                        j.Id, 
                        j.Title, 
                        j.Company, 
                        j.Source, 
                        j.ExternalId,
                        j.CreatedAt,
                        j.Category,
                        j.IsActive
                    })
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync();
                    
                return Ok(new { 
                    TotalJobs = jobs.Count,
                    JobsBySource = jobs.GroupBy(j => j.Source).Select(g => new { Source = g.Key, Count = g.Count() }),
                    Jobs = jobs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        
        [HttpDelete("clear-jobs")]
        public async Task<IActionResult> ClearJobs([FromQuery] string source = "")
        {
            try
            {
                IQueryable<Job> jobsQuery = _context.Jobs;
                
                if (!string.IsNullOrEmpty(source))
                {
                    // Clear jobs from specific source
                    jobsQuery = jobsQuery.Where(j => j.Source == source);
                }
                else
                {
                    // Clear all jobs except manual test jobs
                    jobsQuery = jobsQuery.Where(j => j.Source != "Manual Test");
                }
                
                var jobsToRemove = await jobsQuery.ToListAsync();
                _context.Jobs.RemoveRange(jobsToRemove);
                await _context.SaveChangesAsync();
                
                var remainingJobs = await _context.Jobs.CountAsync();
                
                return Ok(new { 
                    Message = "Jobs cleared successfully", 
                    JobsRemoved = jobsToRemove.Count,
                    RemainingJobs = remainingJobs,
                    ClearedSource = string.IsNullOrEmpty(source) ? "All (except Manual Test)" : source
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        
        [HttpDelete("clear-all-jobs")]
        public async Task<IActionResult> ClearAllJobs()
        {
            try
            {
                var allJobs = await _context.Jobs.ToListAsync();
                _context.Jobs.RemoveRange(allJobs);
                await _context.SaveChangesAsync();
                
                return Ok(new { 
                    Message = "All jobs cleared successfully", 
                    JobsRemoved = allJobs.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        
        [HttpGet("job-stats")]
        public async Task<IActionResult> GetJobStats()
        {
            try
            {
                var totalJobs = await _context.Jobs.CountAsync();
                var activeJobs = await _context.Jobs.CountAsync(j => j.IsActive);
                var jobsBySource = await _context.Jobs
                    .GroupBy(j => j.Source)
                    .Select(g => new { Source = g.Key, Count = g.Count() })
                    .ToListAsync();
                var jobsByCategory = await _context.Jobs
                    .Where(j => j.IsActive)
                    .GroupBy(j => j.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToListAsync();
                var recentJobs = await _context.Jobs
                    .CountAsync(j => j.CreatedAt > DateTime.UtcNow.AddDays(-7));
                
                return Ok(new {
                    TotalJobs = totalJobs,
                    ActiveJobs = activeJobs,
                    RecentJobs = recentJobs,
                    JobsBySource = jobsBySource,
                    JobsByCategory = jobsByCategory
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}