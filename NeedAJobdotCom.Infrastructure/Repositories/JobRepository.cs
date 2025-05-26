using Microsoft.EntityFrameworkCore;
using NeedAJobdotCom.Core.Entities;
using NeedAJobdotCom.Core.Interfaces;
using NeedAJobdotCom.Core.DTOs;
using NeedAJobdotCom.Infrastructure.Data;

namespace NeedAJobdotCom.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly JobBoardContext _context;
        
        public JobRepository(JobBoardContext context)
        {
            _context = context;
        }
        
        public async Task<JobListResponse> GetJobsAsync(JobFilterDto filter)
        {
            var query = _context.Jobs.Where(j => j.IsActive);
            
            // Apply filters
            if (!string.IsNullOrEmpty(filter.Category) && filter.Category != "all")
            {
                query = query.Where(j => j.Category.ToLower() == filter.Category.ToLower());
            }
            
            if (!string.IsNullOrEmpty(filter.Location))
            {
                query = query.Where(j => j.Location.ToLower().Contains(filter.Location.ToLower()) || 
                                        (filter.Location.ToLower() == "remote" && j.IsRemote));
            }
            
            if (!string.IsNullOrEmpty(filter.Type))
            {
                if (Enum.TryParse<JobType>(filter.Type, true, out var jobType))
                {
                    query = query.Where(j => j.Type == jobType);
                }
            }
            
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(j => j.Title.Contains(filter.Search) || 
                                        j.Company.Contains(filter.Search) ||
                                        j.Description.Contains(filter.Search));
            }
            
            // Order by most recent
            query = query.OrderByDescending(j => j.PostedDate);
            
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            
            var jobs = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(j => new JobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Company = j.Company,
                    CompanyLogo = j.CompanyLogo,
                    Description = j.Description.Length > 200 ? j.Description.Substring(0, 200) + "..." : j.Description,
                    Location = j.Location,
                    IsRemote = j.IsRemote,
                    Salary = j.Salary,
                    Type = j.Type.ToString(),
                    Category = j.Category,
                    Tags = j.Tags,
                    ApplyUrl = j.ApplyUrl,
                    Source = j.Source,
                    PostedDate = j.PostedDate,
                    PostedAgo = CalculateTimeAgo(j.PostedDate)
                })
                .ToListAsync();
            
            return new JobListResponse
            {
                Jobs = jobs,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };
        }
        
        public async Task<Job?> GetJobByIdAsync(int id)
        {
            return await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        }
        
        public async Task<Job> CreateJobAsync(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }
        
        public async Task<Job> UpdateJobAsync(Job job)
        {
            job.UpdatedAt = DateTime.UtcNow;
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }
        
        public async Task DeleteJobAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<bool> JobExistsAsync(string externalId, string source)
        {
            return await _context.Jobs.AnyAsync(j => j.ExternalId == externalId && j.Source == source);
        }
        
        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Jobs
                .Where(j => j.IsActive)
                .Select(j => j.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
        
        public async Task<List<string>> GetLocationsAsync()
        {
            return await _context.Jobs
                .Where(j => j.IsActive)
                .Select(j => j.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
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