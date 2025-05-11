using JobsNestApp.Services;

namespace JobsNestApp.Pages
{
    public partial class MessagesPage : ContentPage
    {
        private readonly ApiService _apiService;

        public MessagesPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }
    }
}