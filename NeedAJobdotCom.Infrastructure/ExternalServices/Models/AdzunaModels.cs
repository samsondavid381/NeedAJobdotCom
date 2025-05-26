using System.Text.Json.Serialization;

namespace NeedAJobdotCom.Infrastructure.ExternalServices.Models
{
    // Adzuna API Response Models
    public class AdzunaResponse
    {
        [JsonPropertyName("results")]
        public List<AdzunaJob> Results { get; set; } = new();
        
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
    
    public class AdzunaJob
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("company")]
        public AdzunaCompany Company { get; set; } = new();
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("location")]
        public AdzunaLocation Location { get; set; } = new();
        
        [JsonPropertyName("salary_min")]
        public double? SalaryMin { get; set; }
        
        [JsonPropertyName("salary_max")]
        public double? SalaryMax { get; set; }
        
        [JsonPropertyName("contract_type")]
        public string? ContractType { get; set; }
        
        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }
        
        [JsonPropertyName("category")]
        public AdzunaCategory Category { get; set; } = new();
    }
    
    public class AdzunaCompany
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
    }
    
    public class AdzunaLocation
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
        
        [JsonPropertyName("area")]
        public List<string> Area { get; set; } = new();
    }
    
    public class AdzunaCategory
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;
        
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;
    }
    
    // Internal mapping model
    public class ExternalJobData
    {
        public string ExternalId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsRemote { get; set; }
        public string? Salary { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? ApplyUrl { get; set; }
        public DateTime PostedDate { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}