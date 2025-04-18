namespace JobNestapp.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int UserId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? ResumeUrl { get; set; } 
        public Job? Job { get; set; } 
        public User? User { get; set; } 
    }
}