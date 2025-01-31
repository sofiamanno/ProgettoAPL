namespace ProgettoAPL //definisce lo spazio dei nomi
{
    public partial class App : Application //definisce la classe App che eredita da Application
    {
        public static ApiService ApiServiceInstance { get; private set; } //definisce una proprietà statica di sola lettura di tipo ApiService  
        public App() //cosruttore della classe App
        {


            InitializeComponent(); //inizializza i componenti definiti in App.xaml
            // Inizializza l'istanza singleton di ApiService
            ApiServiceInstance = new ApiService();

            MainPage = new NavigationPage(new ProgettoAPL.Views.LoginPage()); //imposta la pagina principale dell'applicazione, in questo caso la pagina di login
        }
    }
}