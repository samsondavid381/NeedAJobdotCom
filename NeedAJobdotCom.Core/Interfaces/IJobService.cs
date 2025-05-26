using NeedAJobdotCom.Core.DTOs;

namespace NeedAJobdotCom.Core.Interfaces
{
    public interface IJobService
    {
        Task<JobListResponse> GetJobsAsync(JobFilterDto filter);
        Task<JobDto?> GetJobByIdAsync(int id);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetLocationsAsync();
        Task RefreshJobsAsync();
    }
}