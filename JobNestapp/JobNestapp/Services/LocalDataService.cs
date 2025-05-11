using JobsNestApp.Models;
using Microsoft.Data.Sqlite;

namespace JobsNestApp.Services
{
    public class LocalDataService : IDataService
    {
        private readonly string _dbPath;
        private User _currentUser;

        public LocalDataService()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jobsnest.db");
            SeedSampleData();
        }

        private SqliteConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={_dbPath}");
        }

        public async Task<User> RegisterUserAsync(string username, string email, string password)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = $email";
            checkCmd.Parameters.AddWithValue("$email", email);
            var count = (long)await checkCmd.ExecuteScalarAsync();
            if (count > 0) return null;

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Users (Username, Email, Password) 
                VALUES ($username, $email, $password);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$username", username);
            cmd.Parameters.AddWithValue("$email", email);
            cmd.Parameters.AddWithValue("$password", password); // U produkciji hashiraj s BCrypt
            var id = (long)await cmd.ExecuteScalarAsync();

            var user = new User
            {
                Id = (int)id,
                Username = username,
                Email = email,
                Password = password
            };
            _currentUser = user;
            return user;
        }

        public async Task<User> LoginUserAsync(string usernameOrEmail, string password)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT * FROM Users 
                WHERE (Username = $usernameOrEmail OR Email = $usernameOrEmail) 
                AND Password = $password";
            cmd.Parameters.AddWithValue("$usernameOrEmail", usernameOrEmail);
            cmd.Parameters.AddWithValue("$password", password);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    Password = reader.GetString(3)
                };
                _currentUser = user;
                return user;
            }
            return null;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            return await Task.FromResult(_currentUser);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Users WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    Password = reader.GetString(3)
                };
            }
            return null;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Users 
                SET Username = $username, Email = $email, FirstName = $firstName, LastName = $lastName, 
                    PhoneNumber = $phoneNumber, ProfileImage = $profileImage, CV = $cv
                WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", user.Id);
            cmd.Parameters.AddWithValue("$username", user.Username);
            cmd.Parameters.AddWithValue("$email", user.Email);
            cmd.Parameters.AddWithValue("$firstName", user.FirstName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$lastName", user.LastName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$phoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$profileImage", user.ProfileImage ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$cv", user.CV ?? (object)DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UploadUserCVAsync(int userId, string filePath)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Users SET CV = $cv WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", userId);
            cmd.Parameters.AddWithValue("$cv", filePath);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateUserSkillsAsync(int userId, List<string> skills)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;
            user.Skills = skills;
            return await UpdateUserAsync(user);
        }

        public async Task<List<Job>> GetAllJobsAsync(int? categoryId = null, string? location = null, string? searchQuery = null)
        {
            var jobs = new List<Job>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            var conditions = new List<string>();
            if (!string.IsNullOrEmpty(searchQuery))
                conditions.Add("(Title LIKE '%' || $searchQuery || '%' OR Description LIKE '%' || $searchQuery || '%')");
            if (!string.IsNullOrEmpty(location))
                conditions.Add("Location LIKE '%' || $location || '%'");
            if (categoryId.HasValue)
                conditions.Add("Category = $categoryId");

            cmd.CommandText = $"SELECT * FROM Jobs {(conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "")}";
            if (!string.IsNullOrEmpty(searchQuery)) cmd.Parameters.AddWithValue("$searchQuery", searchQuery);
            if (!string.IsNullOrEmpty(location)) cmd.Parameters.AddWithValue("$location", location);
            if (categoryId.HasValue) cmd.Parameters.AddWithValue("$categoryId", categoryId.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var job = new Job
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CompanyId = reader.GetInt32(3),
                    Location = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Type = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Category = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SalaryRange = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PostedDate = DateTime.Parse(reader.GetString(8)),
                    Deadline = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9))
                };
                job.Company = await GetCompanyByIdAsync(job.CompanyId);
                jobs.Add(job);
            }
            return jobs;
        }

        public async Task<Job> GetJobByIdAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Jobs WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var job = new Job
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CompanyId = reader.GetInt32(3),
                    Location = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Type = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Category = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SalaryRange = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PostedDate = DateTime.Parse(reader.GetString(8)),
                    Deadline = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9))
                };
                job.Company = await GetCompanyByIdAsync(job.CompanyId);
                return job;
            }
            return null;
        }

        public async Task<List<Job>> GetRecentJobsAsync(int count = 5)
        {
            var jobs = new List<Job>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Jobs ORDER BY PostedDate DESC LIMIT $count";
            cmd.Parameters.AddWithValue("$count", count);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var job = new Job
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CompanyId = reader.GetInt32(3),
                    Location = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Type = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Category = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SalaryRange = reader.IsDBNull(7) ? null : reader.GetString(7),
                    PostedDate = DateTime.Parse(reader.GetString(8)),
                    Deadline = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9))
                };
                job.Company = await GetCompanyByIdAsync(job.CompanyId);
                jobs.Add(job);
            }
            return jobs;
        }

        public async Task<List<Job>> GetRecommendedJobsAsync(int userId, int count = 5)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null || user.Skills.Count == 0) return await GetRecentJobsAsync(count);

            var allJobs = await GetAllJobsAsync();
            var recommendedJobs = allJobs
                .Where(j => j.Description != null && user.Skills.Any(s => j.Description.Contains(s, StringComparison.OrdinalIgnoreCase)))
                .Take(count)
                .ToList();

            if (recommendedJobs.Count < count)
            {
                var additionalJobs = (await GetRecentJobsAsync(count)).Where(j => !recommendedJobs.Any(rj => rj.Id == j.Id)).Take(count - recommendedJobs.Count);
                recommendedJobs.AddRange(additionalJobs);
            }
            return recommendedJobs;
        }

        public async Task<Company> GetCompanyByIdAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Companies WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Company
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Location = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Website = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Logo = reader.IsDBNull(7) ? null : reader.GetString(7)
                };
            }
            return null;
        }

        public async Task<bool> ApplyForJobAsync(int userId, int jobId, string? coverLetter = null, string? cvPath = null)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM JobApplications WHERE UserId = $userId AND JobId = $jobId";
            checkCmd.Parameters.AddWithValue("$userId", userId);
            checkCmd.Parameters.AddWithValue("$jobId", jobId);
            var count = (long)await checkCmd.ExecuteScalarAsync();
            if (count > 0) return false;

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO JobApplications (JobId, UserId, Status, ApplyDate, CoverLetter, UsedCV)
                VALUES ($jobId, $userId, 'pending', $applyDate, $coverLetter, $usedCV)";
            cmd.Parameters.AddWithValue("$jobId", jobId);
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.Parameters.AddWithValue("$applyDate", DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("$coverLetter", coverLetter ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$usedCV", cvPath ?? (object)DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                var job = await GetJobByIdAsync(jobId);
                await CreateNotificationAsync(userId, "Prijava poslana", $"Uspješno ste se prijavili za poziciju {job.Title}");
                return true;
            }
            return false;
        }

        public async Task<List<JobApplication>> GetUserApplicationsAsync(int userId)
        {
            var applications = new List<JobApplication>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM JobApplications WHERE UserId = $userId";
            cmd.Parameters.AddWithValue("$userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var application = new JobApplication
                {
                    Id = reader.GetInt32(0),
                    JobId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    ApplyDate = DateTime.Parse(reader.GetString(4)),
                    CoverLetter = reader.IsDBNull(5) ? null : reader.GetString(5),
                    UsedCV = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
                application.Job = await GetJobByIdAsync(application.JobId);
                application.User = await GetUserByIdAsync(application.UserId);
                applications.Add(application);
            }
            return applications;
        }

        public async Task<JobApplication> GetApplicationByIdAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM JobApplications WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var application = new JobApplication
                {
                    Id = reader.GetInt32(0),
                    JobId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    ApplyDate = DateTime.Parse(reader.GetString(4)),
                    CoverLetter = reader.IsDBNull(5) ? null : reader.GetString(5),
                    UsedCV = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
                application.Job = await GetJobByIdAsync(application.JobId);
                application.User = await GetUserByIdAsync(application.UserId);
                return application;
            }
            return null;
        }

        public async Task<bool> SendMessageAsync(int senderId, int receiverId, string content)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Messages (SenderId, ReceiverId, Content, SentDate, IsRead)
                VALUES ($senderId, $receiverId, $content, $sentDate, 0)";
            cmd.Parameters.AddWithValue("$senderId", senderId);
            cmd.Parameters.AddWithValue("$receiverId", receiverId);
            cmd.Parameters.AddWithValue("$content", content);
            cmd.Parameters.AddWithValue("$sentDate", DateTime.Now.ToString("o"));

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                var sender = await GetUserByIdAsync(senderId);
                await CreateNotificationAsync(receiverId, "Nova poruka", $"Primili ste novu poruku od {sender.Username}");
                return true;
            }
            return false;
        }

        public async Task<List<Message>> GetConversationAsync(int user1Id, int user2Id)
        {
            var messages = new List<Message>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT * FROM Messages 
                WHERE (SenderId = $user1Id AND ReceiverId = $user2Id) 
                OR (SenderId = $user2Id AND ReceiverId = $user1Id)
                ORDER BY SentDate";
            cmd.Parameters.AddWithValue("$user1Id", user1Id);
            cmd.Parameters.AddWithValue("$user2Id", user2Id);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var message = new Message
                {
                    Id = reader.GetInt32(0),
                    SenderId = reader.GetInt32(1),
                    ReceiverId = reader.GetInt32(2),
                    Content = reader.GetString(3),
                    SentDate = DateTime.Parse(reader.GetString(4)),
                    IsRead = reader.GetInt32(5) == 1
                };
                message.Sender = await GetUserByIdAsync(message.SenderId);
                message.Receiver = await GetUserByIdAsync(message.ReceiverId);
                messages.Add(message);
            }
            return messages;
        }

        public async Task<List<User>> GetUserConversationsAsync(int userId)
        {
            var conversationUserIds = new List<int>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT CASE 
                    WHEN SenderId = $userId THEN ReceiverId 
                    ELSE SenderId 
                END AS OtherUserId 
                FROM Messages 
                WHERE SenderId = $userId OR ReceiverId = $userId";
            cmd.Parameters.AddWithValue("$userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conversationUserIds.Add(reader.GetInt32(0));
            }

            var users = new List<User>();
            foreach (var id in conversationUserIds)
            {
                var user = await GetUserByIdAsync(id);
                if (user != null) users.Add(user);
            }
            return users;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            var notifications = new List<Notification>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Notifications WHERE UserId = $userId ORDER BY CreatedDate DESC";
            cmd.Parameters.AddWithValue("$userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notifications.Add(new Notification
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Content = reader.GetString(3),
                    CreatedDate = DateTime.Parse(reader.GetString(4)),
                    IsRead = reader.GetInt32(5) == 1
                });
            }
            return notifications;
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Notifications SET IsRead = 1 WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", notificationId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> CreateNotificationAsync(int userId, string title, string content)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Notifications (UserId, Title, Content, CreatedDate, IsRead)
                VALUES ($userId, $title, $content, $createdDate, 0)";
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.Parameters.AddWithValue("$title", title);
            cmd.Parameters.AddWithValue("$content", content);
            cmd.Parameters.AddWithValue("$createdDate", DateTime.Now.ToString("o"));

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            var companies = new List<Company>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Companies";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                companies.Add(new Company
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Location = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Website = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Logo = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return companies;
        }

        public async Task<bool> CreateJobAsync(Job job)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Jobs (Title, Description, CompanyId, Location, Type, Category, SalaryRange, PostedDate, Deadline)
                VALUES ($title, $description, $companyId, $location, $type, $category, $salaryRange, $postedDate, $deadline)";
            cmd.Parameters.AddWithValue("$title", job.Title);
            cmd.Parameters.AddWithValue("$description", job.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$companyId", job.CompanyId);
            cmd.Parameters.AddWithValue("$location", job.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$type", job.Type ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$category", job.Category ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$salaryRange", job.SalaryRange ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$postedDate", job.PostedDate.ToString("o"));
            cmd.Parameters.AddWithValue("$deadline", job.Deadline?.ToString("o") ?? (object)DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateJobAsync(Job job)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Jobs 
                SET Title = $title, Description = $description, CompanyId = $companyId, 
                    Location = $location, Type = $type, Category = $category, 
                    SalaryRange = $salaryRange, PostedDate = $postedDate, Deadline = $deadline
                WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", job.Id);
            cmd.Parameters.AddWithValue("$title", job.Title);
            cmd.Parameters.AddWithValue("$description", job.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$companyId", job.CompanyId);
            cmd.Parameters.AddWithValue("$location", job.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$type", job.Type ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$category", job.Category ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$salaryRange", job.SalaryRange ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$postedDate", job.PostedDate.ToString("o"));
            cmd.Parameters.AddWithValue("$deadline", job.Deadline?.ToString("o") ?? (object)DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Jobs WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        private void SeedSampleData()
        {
            using var connection = GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Users";
            var userCount = (long)cmd.ExecuteScalar();
            if (userCount > 0) return;

            // Test korisnik
            cmd.CommandText = @"
                INSERT INTO Users (Username, Email, Password, FirstName, LastName, PhoneNumber)
                VALUES ('Imad', 'imad@example.com', 'password123', 'Imad', 'Prezime', '+387 61 123 456')";
            cmd.ExecuteNonQuery();

            // Kompanije
            cmd.CommandText = @"
                INSERT INTO Companies (Name, Description, Location, PhoneNumber, Email, Website, Logo)
                VALUES ('Five', 'Vodeća IT kompanija za mobile development', 'Sarajevo, BiH', '+385 61 778 253', 'five.hr@gmail.com', 'five.studio', 'five_logo.png')";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
                INSERT INTO Companies (Name, Description, Location, PhoneNumber, Email, Website, Logo)
                VALUES ('Symphony', 'Softverska kompanija specijalizirana za razvoj enterprise rješenja', 'Sarajevo, BiH', '+387 61 890 123', 'info@symphony.ba', 'symphony.ba', 'symphony_logo.png')";
            cmd.ExecuteNonQuery();

            // Poslovi
            cmd.CommandText = @"
                INSERT INTO Jobs (Title, Description, CompanyId, Location, Type, Category, SalaryRange, PostedDate)
                VALUES ('Web & Mobile Development', 'Tražimo iskusne developere za rad na digitalnim agencijskim projektima...', 1, 'Sarajevo, BiH', 'Full-time', 'Development', '3000 KM - 5000 KM', $postedDate)";
            cmd.Parameters.AddWithValue("$postedDate", DateTime.Now.AddDays(-5).ToString("o"));
            cmd.ExecuteNonQuery();
        }
    }
}