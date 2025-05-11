using JobsNestApp.Services;

namespace JobsNestApp.Pages
{
    public partial class SignupPage : ContentPage
    {
        private readonly ApiService _apiService;

        public SignupPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnSignupClicked(object sender, EventArgs e)
        {
            var user = await _apiService.RegisterAsync(UsernameEntry.Text, EmailEntry.Text, PasswordEntry.Text);
            if (user?.Id > 0)
            {
                await Navigation.PopAsync(); // Vrati se na LoginPage
            }
            else
            {
                ErrorLabel.Text = "Registracija nije uspjela. Provjeri podatke.";
            }
        }
    }
}