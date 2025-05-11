using JobsNestApp.Models;

namespace JobsNestApp.Services
{
    public class ApiService
    {
        private readonly IDataService _dataService;
        private User _currentUser;

        public ApiService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            var user = await _dataService.RegisterUserAsync(username, email, password);
            if (user != null) _currentUser = user;
            return user;
        }

        public async Task<User> LoginAsync(string usernameOrEmail, string password)
        {
            var user = await _dataService.LoginUserAsync(usernameOrEmail, password);
            if (user != null) _currentUser = user;
            return user;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            return await _dataService.GetCurrentUserAsync();
        }

        public async Task<List<Job>> GetRecentJobsAsync()
        {
            return await _dataService.GetRecentJobsAsync();
        }

        public async Task<List<Job>> SearchJobsAsync(string query = null, string location = null)
        {
            return await _dataService.GetAllJobsAsync(searchQuery: query, location: location);
        }

        public async Task<List<JobApplication>> GetMyApplicationsAsync()
        {
            return _currentUser != null ? await _dataService.GetUserApplicationsAsync(_currentUser.Id) : new List<JobApplication>();
        }

        public async Task<bool> ApplyForJobAsync(int jobId, string coverLetter = null, string cvPath = null)
        {
            return _currentUser != null && await _dataService.ApplyForJobAsync(_currentUser.Id, jobId, coverLetter, cvPath);
        }

        public async Task<bool> CreateJobAsync(Job job)
        {
            return await _dataService.CreateJobAsync(job);
        }

        public async Task<bool> UpdateJobAsync(Job job)
        {
            return await _dataService.UpdateJobAsync(job);
        }

        public async Task<bool> DeleteJobAsync(int jobId)
        {
            return await _dataService.DeleteJobAsync(jobId);
        }

        public async Task<Job> GetJobDetailsAsync(int jobId)
        {
            return await _dataService.GetJobByIdAsync(jobId);
        }

        public async Task<bool> UpdateUserProfileAsync(User updatedUser)
        {
            return await _dataService.UpdateUserAsync(updatedUser);
        }

        public async Task<bool> UploadCVAsync(string filePath)
        {
            if (_currentUser == null) return false;
            return await _dataService.UploadUserCVAsync(_currentUser.Id, filePath);
        }
    }
}