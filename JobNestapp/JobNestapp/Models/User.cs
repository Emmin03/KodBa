// Models/User.cs
namespace JobsNestApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfileImage { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public string? CV { get; set; }
    }
}