// Models/JobApplication.cs


namespace JobsNestApp.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime ApplyDate { get; set; }
        public string? CoverLetter { get; set; }
        public string? UsedCV { get; set; }
        public Job? Job { get; set; }
        public User? User { get; set; }
    }
}