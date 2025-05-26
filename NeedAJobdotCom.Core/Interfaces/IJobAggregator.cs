using NeedAJobdotCom.Core.Entities;

namespace NeedAJobdotCom.Core.Interfaces
{
    public interface IJobAggregator
    {
        Task<List<Job>> FetchJobsAsync(string category = "", int limit = 100);
        Task<int> RefreshAllCategoriesAsync();
        Task<int> RefreshCategoryAsync(string category, int limit = 50);
    }
}