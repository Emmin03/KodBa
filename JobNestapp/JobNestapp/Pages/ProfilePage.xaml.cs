using JobsNestApp.Services;

namespace JobsNestApp.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ApiService _apiService;

        public ProfilePage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnUpdateProfileClicked(object sender, EventArgs e)
        {
            var user = await _apiService.GetCurrentUserAsync();
            if (user != null)
            {
                ProfileLabel.Text = $"Korisnik: {user.Username}, Email: {user.Email}";
                // Ovdje možeš dodati logiku za ažuriranje profila
            }
        }
    }
}