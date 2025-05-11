using JobsNestApp.Services;

namespace JobsNestApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var user = await _apiService.LoginAsync(EmailEntry.Text, PasswordEntry.Text);
            if (user?.Id > 0)
            {
                await Navigation.PushAsync(new AppShell());
            }
            else
            {
                ErrorLabel.Text = "Neispravni podaci za prijavu.";
            }
        }

        private async void OnSignupClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignupPage(_apiService));
        }
    }
}