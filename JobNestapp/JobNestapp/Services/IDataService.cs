using JobsNestApp.Models;

namespace JobsNestApp.Services
{
    public interface IDataService
    {
        Task<User> RegisterUserAsync(string username, string email, string password);
        Task<User> LoginUserAsync(string usernameOrEmail, string password);
        Task<User> GetCurrentUserAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UploadUserCVAsync(int userId, string filePath);
        Task<bool> UpdateUserSkillsAsync(int userId, List<string> skills);
        Task<List<Job>> GetAllJobsAsync(int? categoryId = null, string? location = null, string? searchQuery = null);
        Task<Job> GetJobByIdAsync(int id);
        Task<List<Job>> GetRecentJobsAsync(int count = 5);
        Task<List<Job>> GetRecommendedJobsAsync(int userId, int count = 5);
        Task<Company> GetCompanyByIdAsync(int id);
        Task<bool> ApplyForJobAsync(int userId, int jobId, string? coverLetter = null, string? cvPath = null);
        Task<List<JobApplication>> GetUserApplicationsAsync(int userId);
        Task<JobApplication> GetApplicationByIdAsync(int id);
        Task<bool> SendMessageAsync(int senderId, int receiverId, string content);
        Task<List<Message>> GetConversationAsync(int user1Id, int user2Id);
        Task<List<User>> GetUserConversationsAsync(int userId);
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> CreateNotificationAsync(int userId, string title, string content);
        Task<List<Company>> GetAllCompaniesAsync();
        Task<bool> CreateJobAsync(Job job);
        Task<bool> UpdateJobAsync(Job job);
        Task<bool> DeleteJobAsync(int id);
    }
}