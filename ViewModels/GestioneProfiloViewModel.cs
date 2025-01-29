using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.Views;
using ProgettoAPL.Services;



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
            await ExecuteWithExceptionHandling(async () =>
            {
                var sessionCookie = GetSessionCookie();
                if (sessionCookie != null)
                {
                    AddSessionCookieToHeaders(sessionCookie);
                    Utente = await _apiService.GetProfileAsync();
                }
                else
                {
                    await ShowAlert("Errore", "Nessun cookie di sessione trovato.");
                }
            });
        }

        private async Task OnConfermaPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NuovaPassword) || NuovaPassword.Length < 6)
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
                    // Pulisci i campi del form
                    NuovaPassword = string.Empty;
                    ConfermaPassword = string.Empty;
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

        private Cookie GetSessionCookie()
        {
            var cookies = SessionManager.GetSessionCookies();
            return cookies.FirstOrDefault(c => c.Name == "session-name");
        }

        private void AddSessionCookieToHeaders(Cookie sessionCookie)
        {
            if (_apiService.HttpClient.DefaultRequestHeaders.Contains("Cookie"))
            {
                _apiService.HttpClient.DefaultRequestHeaders.Remove("Cookie");
            }
            _apiService.HttpClient.DefaultRequestHeaders.Add("Cookie", $"{sessionCookie.Name}={sessionCookie.Value}");
        }

        private async Task ExecuteWithExceptionHandling(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (HttpRequestException ex)
            {
                await ShowAlert("Errore", $"Errore durante la richiesta: {ex.Message}");
            }
            catch (Exception ex)
            {
                await ShowAlert("Errore", $"Errore imprevisto: {ex.Message}");
            }
        }

        private async Task ShowAlert(string title, string message)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
