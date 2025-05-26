using Microsoft.AspNetCore.Mvc;
using NeedAJobdotCom.Core.Interfaces;
using NeedAJobdotCom.Core.DTOs;

namespace NeedAJobdotCom.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IJobAggregator _jobAggregator;
        private readonly ILogger<JobsController> _logger;
        
        public JobsController(IJobService jobService, IJobAggregator jobAggregator, ILogger<JobsController> logger)
        {
            _jobService = jobService;
            _jobAggregator = jobAggregator;
            _logger = logger;
        }
        
        /// <summary>
        /// Get jobs with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<JobListResponse>> GetJobs([FromQuery] JobFilterDto filter)
        {
            try
            {
                var jobs = await _jobService.GetJobsAsync(filter);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs with filter: {@Filter}", filter);
                return StatusCode(500, new { Error = "Failed to retrieve jobs" });
            }
        }
        
        /// <summary>
        /// Get a specific job by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<JobDto>> GetJob(int id)
        {
            try
            {
                var job = await _jobService.GetJobByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { Error = $"Job with ID {id} not found" });
                }
                
                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job with ID: {JobId}", id);
                return StatusCode(500, new { Error = "Failed to retrieve job" });
            }
        }
        
        /// <summary>
        /// Get available job categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            try
            {
                var categories = await _jobService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { Error = "Failed to retrieve categories" });
            }
        }
        
        /// <summary>
        /// Get available job locations
        /// </summary>
        [HttpGet("locations")]
        public async Task<ActionResult<List<string>>> GetLocations()
        {
            try
            {
                var locations = await _jobService.GetLocationsAsync();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations");
                return StatusCode(500, new { Error = "Failed to retrieve locations" });
            }
        }
        
        /// <summary>
        /// Manually refresh jobs from external APIs
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshJobs([FromQuery] string? category = null, [FromQuery] int limit = 50)
        {
            try
            {
                int jobsAdded;
                
                if (string.IsNullOrEmpty(category))
                {
                    _logger.LogInformation("Starting refresh of all job categories");
                    jobsAdded = await _jobAggregator.RefreshAllCategoriesAsync();
                }
                else
                {
                    _logger.LogInformation("Starting refresh of category: {Category}", category);
                    jobsAdded = await _jobAggregator.RefreshCategoryAsync(category, limit);
                }
                
                return Ok(new { 
                    Message = $"Successfully refreshed jobs", 
                    JobsAdded = jobsAdded,
                    Category = category ?? "all"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing jobs for category: {Category}", category);
                return StatusCode(500, new { Error = "Failed to refresh jobs" });
            }
        }
        
        /// <summary>
        /// Get job statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetJobStats()
        {
            try
            {
                var allJobs = await _jobService.GetJobsAsync(new JobFilterDto { PageSize = int.MaxValue });
                
                var stats = new
                {
                    TotalJobs = allJobs.TotalCount,
                    JobsByCategory = allJobs.Jobs
                        .GroupBy(j => j.Category)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    JobsByType = allJobs.Jobs
                        .GroupBy(j => j.Type)
                        .Select(g => new { Type = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    RemoteJobs = allJobs.Jobs.Count(j => j.IsRemote),
                    RecentJobs = allJobs.Jobs.Count(j => j.PostedDate > DateTime.UtcNow.AddDays(-7))
                };
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job statistics");
                return StatusCode(500, new { Error = "Failed to retrieve job statistics" });
            }
        }
    }
}