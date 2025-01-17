namespace ProgettoAPL
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new ProgettoAPL.Views.LoginPage());
        }
    }
}