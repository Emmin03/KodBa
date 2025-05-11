// Models/Job.cs
namespace JobsNestApp.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public string? Location { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? SalaryRange { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public Company? Company { get; set; }
    }
}