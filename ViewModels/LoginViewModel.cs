using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProgettoAPL.Views;
using System.Net;

namespace ProgettoAPL.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _email;
        private string _password;
        private bool _isLoggingIn;
        private readonly ApiService _apiService;

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                if (_isLoggingIn != value)
                {
                    _isLoggingIn = value;
                    OnPropertyChanged(nameof(IsLoggingIn));
                }
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await OnLogin());
        }

        private async Task OnLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "I campi non possono essere vuoti.", "OK");
                return;
            }

            if (!Email.Contains("@"))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Email non valida.", "OK");
                return;
            }

            IsLoggingIn = true;
            try
            {
                await _apiService.LoginUserAsync(Email, Password);
                // Assuming the LoginUserAsync method throws an exception on failure
                // Naviga alla pagina principale o dashboard
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Credenziali non valide. Riprova.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", $"Si è verificato un errore: {ex.Message}", "OK");
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
