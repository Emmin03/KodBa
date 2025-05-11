using JobsNestApp.Services;

namespace JobsNestApp.Pages
{
    public partial class SearchPage : ContentPage
    {
        private readonly ApiService _apiService;

        public SearchPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            var jobs = await _apiService.SearchJobsAsync(QueryEntry.Text, LocationEntry.Text);
            ResultsLabel.Text = string.Join("\n", jobs.Select(j => j.Title));
        }
    }
}