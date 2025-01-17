using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.Views;


namespace ProgettoAPL.ViewModels
{
    public class GestioneProfiloViewModel : INotifyPropertyChanged
    {
        private string _nuovaPassword;
        private string _confermaPassword;
        private Utente _utente;
        private readonly ApiService _apiService;

        public string NuovaPassword
        {
            get => _nuovaPassword;
            set
            {
                if (_nuovaPassword != value)
                {
                    _nuovaPassword = value;
                    OnPropertyChanged(nameof(NuovaPassword));
                }
            }
        }

        public string ConfermaPassword
        {
            get => _confermaPassword;
            set
            {
                if (_confermaPassword != value)
                {
                    _confermaPassword = value;
                    OnPropertyChanged(nameof(ConfermaPassword));
                }
            }
        }

        public Utente Utente
        {
            get => _utente;
            set
            {
                _utente = value;
                OnPropertyChanged(nameof(Utente));
            }
        }

        public ICommand ConfermaPasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public GestioneProfiloViewModel()
        {
            _apiService = new ApiService();
            ConfermaPasswordCommand = new Command(async () => await OnConfermaPasswordAsync());
            LogoutCommand = new Command(async () => await OnLogoutAsync());

            // Carica i dati del profilo utente
            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            try
            {
                // Recupera il profilo utente senza usare le preferenze
                Utente = await _apiService.GetProfileAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", $"Impossibile caricare il profilo: {ex.Message}", "OK");
            }
        }

        private async Task OnConfermaPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NuovaPassword) || NuovaPassword.Length < 10)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "La password deve avere almeno 10 caratteri", "OK");
                return;
            }

            if (NuovaPassword != ConfermaPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Le password non coincidono", "OK");
                return;
            }

            try
            {
                Utente.Pwd = NuovaPassword;
                var response = await _apiService.UpdateProfileAsync(Utente);
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Successo", "Profilo aggiornato con successo", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Errore", "Impossibile aggiornare il profilo", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", $"Si è verificato un errore: {ex.Message}", "OK");
            }
        }

        private async Task OnLogoutAsync()
        {
            bool conferma = await Application.Current.MainPage.DisplayAlert(
                 "Logout", "Sei sicuro di voler uscire?", "Sì", "No");

            if (conferma)
            {
                // Navigazione alla pagina di Login
                await Application.Current.MainPage.Navigation.PushAsync(new LoginPage());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
