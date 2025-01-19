namespace ProgettoAPL
{
    public partial class App : Application
    {
        public static ApiService ApiServiceInstance { get; private set; }
        public App()
        {


            InitializeComponent();
            // Inizializza l'istanza singleton di ApiService
            ApiServiceInstance = new ApiService();

            MainPage = new NavigationPage(new ProgettoAPL.Views.LoginPage());
        }
    }
}