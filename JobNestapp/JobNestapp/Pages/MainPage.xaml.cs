using JobsNestApp.Models;
using JobsNestApp.Services;

namespace JobsNestApp.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;

        public MainPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnShowRecentJobsClicked(object sender, EventArgs e)
        {
            var jobs = await _apiService.GetRecentJobsAsync();
            JobsLabel.Text = string.Join("\n", jobs.Select(j => j.Title));
        }

        private async void OnCreateJobClicked(object sender, EventArgs e)
        {
            var job = new Job
            {
                Title = "Novi Posao",
                Description = "Opis novog posla",
                CompanyId = 1,
                Location = "Sarajevo",
                Type = "Full-time",
                Category = "Development",
                SalaryRange = "2000 KM - 3000 KM",
                PostedDate = DateTime.Now
            };
            var success = await _apiService.CreateJobAsync(job);
            await DisplayAlert("Rezultat", success ? "Posao kreiran!" : "Greška pri kreiranju posla.", "OK");
        }
    }
}