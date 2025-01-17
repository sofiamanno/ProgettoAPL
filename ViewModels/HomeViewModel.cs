using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.Views;


namespace ProgettoAPL.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }
        public class Progetto
        {
            public string Nome { get; set; }
            public string CreatoDa { get; set; }
        }
        public ObservableCollection<Progetto> Progetti { get; set; }

        // Comandi statici per ora
        public ICommand LogoutCommand { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand ProfileCommand { get; }

        public HomeViewModel()
        {
            // Dati statici per testare l'interfaccia (da rendere dinamici in futuro)
            Progetti = new ObservableCollection<Progetto>
            {
                new Progetto { Nome = "Progetto Alpha", CreatoDa = "Mario Rossi" },
                new Progetto { Nome = "Progetto Beta", CreatoDa = "Giulia Bianchi" }
            };

            // Comandi collegati a metodi statici per ora
            LogoutCommand = new Command(OnLogout);
            NewProjectCommand = new Command(OnNewProject);
            ProfileCommand = new Command(OnProfile);
        }

        // Metodi statici con commenti per future chiamate al server
        private async void OnLogout()
        {
            bool conferma = await Application.Current.MainPage.DisplayAlert(
                  "Logout", "Sei sicuro di voler uscire?", "Sì", "No");

            if (conferma)
            {
                // Reset eventuali dati utente (se presenti)
                //Application.Current.Properties.Clear();
                //await Application.Current.SavePropertiesAsync();

                // Navigazione alla pagina di Login
                await Application.Current.MainPage.Navigation.PushAsync(new LoginPage());
            }
        }

        private void OnNewProject()
        {
            // TODO: Collegare questa funzione con la creazione effettiva di un progetto sul server
            Application.Current.MainPage.DisplayAlert("Crea Nuovo Progetto", "Progetto creato.", "OK");
        }

        private async void OnProfile()
        {
            // Naviga alla pagina di gestione del profilo
            await Application.Current.MainPage.Navigation.PushAsync(new GestioneProfiloPage());
        }

        // Implementazione INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
