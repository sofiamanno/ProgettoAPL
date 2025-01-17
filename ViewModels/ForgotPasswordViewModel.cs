using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ProgettoAPL.ViewModels
{
    public class ForgotPasswordViewModel : INotifyPropertyChanged
    {
        private string email;
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
            SendCommand = new Command(OnSend);
        }

        private async void OnSend()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Inserisci Email.", "OK");
                return;
            }
            else if (!Email.Contains("@"))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "Email non valida.", "OK");
                return;
            }
            // Logica per inviare l'email di recupero password
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}