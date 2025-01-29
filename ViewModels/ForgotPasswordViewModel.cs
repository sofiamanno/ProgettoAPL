using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Net.Http;

namespace ProgettoAPL.ViewModels
{
    public class ForgotPasswordViewModel : INotifyPropertyChanged
    {
        private string email;
        private readonly ApiService _apiService;

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public ICommand SendCommand { get; }

        public ForgotPasswordViewModel()
        {
            _apiService = new ApiService();
            SendCommand = new Command(async () => await OnSend());
        }

        private async Task OnSend()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Inserisci Email.", "OK");
                return;
            }
            else if (!IsValidEmail(Email))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Email non valida.", "OK");
                return;
            }

            // Invia la richiesta GET al server tramite ApiService
            try
            {
                var response = await _apiService.ForgotPasswordAsync(Email);
                if (response)
                {
                    await Application.Current.MainPage.DisplayAlert("Successo", "Email di recupero inviata con successo.", "OK");
                    Email = string.Empty;

                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Errore", "Errore durante l'invio dell'email di recupero.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", $"Si è verificato un errore: {ex.Message}", "OK");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}