using JobsNestApp.Services;

namespace JobsNestApp.Pages
{
    public partial class ApplicationsPage : ContentPage
    {
        private readonly ApiService _apiService;

        public ApplicationsPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnApplyClicked(object sender, EventArgs e)
        {
            var success = await _apiService.ApplyForJobAsync(1); // Pretpostavljeni jobId = 1
            await DisplayAlert("Rezultat", success ? "Prijavljeno!" : "Greška pri prijavi.", "OK");
        }

        private async void OnShowApplicationsClicked(object sender, EventArgs e)
        {
            var applications = await _apiService.GetMyApplicationsAsync();
            ApplicationsLabel.Text = string.Join("\n", applications.Select(a => $"Posao: {a.JobId}, Status: {a.Status}"));
        }
    }
}