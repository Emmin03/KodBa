// Models/Company.cs
namespace JobsNestApp.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
    }
}