using JobsNestApp.Pages;

namespace JobsNestApp
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainPage = new NavigationPage(serviceProvider.GetService<LoginPage>());
        }
    }
}