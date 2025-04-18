namespace JobNestapp.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public decimal Salary { get; set; }
        public string? JobType { get; set; }
        public int EmployerId { get; set; }
        public User? Employer { get; set; }
        public DateTime PostedDate { get; set; } 
    }
}