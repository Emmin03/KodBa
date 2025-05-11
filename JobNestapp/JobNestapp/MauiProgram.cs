using CommunityToolkit.Maui;
using JobsNestApp.Pages;
using JobsNestApp.Services;
using Microsoft.Data.Sqlite;

namespace JobsNestApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registruj servise
            builder.Services.AddSingleton<IDataService, LocalDataService>();
            builder.Services.AddSingleton<ApiService>();

            // Registruj stranice
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignupPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<ApplicationsPage>();
            builder.Services.AddTransient<MessagesPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<AppShell>();

            // Inicijalizacija SQLite baze
            SetupDatabase();

            return builder.Build();
        }

        private static void SetupDatabase()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jobsnest.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    FirstName TEXT,
                    LastName TEXT,
                    PhoneNumber TEXT,
                    ProfileImage TEXT,
                    CV TEXT
                );
                CREATE TABLE IF NOT EXISTS Companies (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Location TEXT,
                    PhoneNumber TEXT,
                    Email TEXT,
                    Website TEXT,
                    Logo TEXT
                );
                CREATE TABLE IF NOT EXISTS Jobs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    CompanyId INTEGER,
                    Location TEXT,
                    Type TEXT,
                    Category TEXT,
                    SalaryRange TEXT,
                    PostedDate TEXT,
                    Deadline TEXT
                );
                CREATE TABLE IF NOT EXISTS JobApplications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    JobId INTEGER,
                    UserId INTEGER,
                    Status TEXT,
                    ApplyDate TEXT,
                    CoverLetter TEXT,
                    UsedCV TEXT
                );
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SenderId INTEGER,
                    ReceiverId INTEGER,
                    Content TEXT NOT NULL,
                    SentDate TEXT,
                    IsRead INTEGER
                );
                CREATE TABLE IF NOT EXISTS Notifications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    CreatedDate TEXT,
                    IsRead INTEGER
                );";
            command.ExecuteNonQuery();
        }
    }
}