using NeedAJobdotCom.Core.Entities;
using NeedAJobdotCom.Core.DTOs;

namespace NeedAJobdotCom.Core.Interfaces
{
    public interface IJobRepository
    {
        Task<JobListResponse> GetJobsAsync(JobFilterDto filter);
        Task<Job?> GetJobByIdAsync(int id);
        Task<Job> CreateJobAsync(Job job);
        Task<Job> UpdateJobAsync(Job job);
        Task DeleteJobAsync(int id);
        Task<bool> JobExistsAsync(string externalId, string source);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetLocationsAsync();
    }
}