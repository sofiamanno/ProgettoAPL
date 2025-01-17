using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;


namespace ProgettoAPL.ViewModels


{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _email;
        private string _password;
        private string _repeatPassword;
        private bool _isRegistering;
        private readonly ApiService _apiService;

        // Implementazione corretta di INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;


        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string RepeatPassword
        {
            get => _repeatPassword;
            set => SetProperty(ref _repeatPassword, value);
        }

        public bool IsRegistering
        {
            get => _isRegistering;
            set => SetProperty(ref _isRegistering, value);
        }

        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            _apiService = new ApiService();
            RegisterCommand = new Command(async () => await RegisterUserAsync());
        }

        // Metodo per la registrazione
        public async Task RegisterUserAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(RepeatPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Tutti i campi devono essere riempiti.", "OK");
                return;
            }

            if (!IsValidEmail(Email))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "L'email inserita non è valida.", "OK");
                return;
            }

            bool usernameExists = await _apiService.CheckUsernameAvailabilityAsync(Username);
            if (usernameExists)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "L'Username selezionato è già in uso.", "OK");
                return;
            }

            if (Password != RepeatPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Le password non corrispondono.", "OK");
                return;
            }

            if (Password.Length < 10)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "La password è troppo corta.", "OK");
                return;
            }

            IsRegistering = true;
            try
            {
                var response = await _apiService.RegisterUserAsync(Username, Email, Password);
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Successo", "Registrazione completata!", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Errore", "Registrazione fallita. Riprova.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", $"Si è verificato un errore: {ex.Message}", "OK");
            }
            finally
            {
                IsRegistering = false;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Usa Regex per validare l'email
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return regex.IsMatch(email);
            }
            catch (Exception)
            {
                return false;
            }
        }


        // Metodo generico per notificare il binding
        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

        

}
