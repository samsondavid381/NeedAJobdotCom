namespace NeedAJobdotCom.Core.DTOs
{
     public class JobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsRemote { get; set; }
        public string? Salary { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string? ApplyUrl { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime PostedDate { get; set; }
        public string PostedAgo { get; set; } = string.Empty;
    }
    
    public class JobFilterDto
    {
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? Type { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    
    public class JobListResponse
    {
        public List<JobDto> Jobs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}