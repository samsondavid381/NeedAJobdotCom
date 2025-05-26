using System.ComponentModel.DataAnnotations;

namespace NeedAJobdotCom.Core.Entities
{
    public class Job
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [MaxLength(200)]
        public string Company { get; set; } = string.Empty;

        public string? CompanyLogo { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        public bool IsRemote { get; set; }

        public string? Salary { get; set; }

        [Required]
        public JobType Type { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();

        public string? ApplyUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string Source { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // For duplicate prevention
        public string? ExternalId { get; set; }
    }
    public enum JobType
    {
        FullTime,
        PartTime,
        Contract,
        Internship,
        Temporary
    }
}