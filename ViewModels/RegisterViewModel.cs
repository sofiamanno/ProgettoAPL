using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;
using ProgettoAPL.Views;


namespace ProgettoAPL.ViewModels


{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _username;
        private string _email;
        private string _password;
        private string _repeatPassword;
        private bool _isRegistering;

        public RegisterViewModel()
        {
            _apiService = new ApiService();
            RegisterCommand = new Command(async () => await RegisterUserAsync(), () => !IsRegistering);
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string RepeatPassword
        {
            get => _repeatPassword;
            set
            {
                _repeatPassword = value;
                OnPropertyChanged(nameof(RepeatPassword));
            }
        }

        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                _isRegistering = value;
                OnPropertyChanged(nameof(IsRegistering));
                ((Command)RegisterCommand).ChangeCanExecute();
            }
        }

        public ICommand RegisterCommand { get; }

        private async Task RegisterUserAsync()
        {
            // Validazioni
            if (string.IsNullOrWhiteSpace(Username))
            {
                await App.Current.MainPage.DisplayAlert("Errore", "Il campo Nome è obbligatorio.", "OK");
                return;
            }

            var isUsernameAvailable = await _apiService.CheckUsernameAvailabilityAsync(Username);
            if (!isUsernameAvailable)
            {
                await App.Current.MainPage.DisplayAlert("Errore", "Il nome utente è già in uso.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await App.Current.MainPage.DisplayAlert("Errore", "Inserisci un'email valida.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                await App.Current.MainPage.DisplayAlert("Errore", "La password deve essere di almeno 6 caratteri.", "OK");
                return;
            }

            if (Password != RepeatPassword)
            {
                await App.Current.MainPage.DisplayAlert("Errore", "Le password non coincidono.", "OK");
                return;
            }

            IsRegistering = true;

            try
            {
                var response = await _apiService.RegisterUserAsync(Username, Email, Password);
                if (response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Successo", "Registrazione avvenuta con successo.", "OK");
                    await Application.Current.MainPage.Navigation.PushAsync(new LoginPage());

                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    await App.Current.MainPage.DisplayAlert("Errore", $"Registrazione fallita: {errorMessage}", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Errore", $"Si è verificato un errore: {ex.Message}", "OK");
            }
            finally
            {
                IsRegistering = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

        

}
