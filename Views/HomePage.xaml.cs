using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProgettoAPL.ViewModels;

namespace ProgettoAPL.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            BindingContext = new HomeViewModel();
        }

        private async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            // Logica di logout
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnAddProjectButtonClicked(object sender, EventArgs e)
        {
            // Logica per aggiungere un nuovo progetto
            await DisplayAlert("Nuovo Progetto", "Aggiungi un nuovo progetto", "OK");
        }

        private async void OnNewProjectButtonClicked(object sender, EventArgs e)
        {
            // Logica per creare un nuovo progetto
            await DisplayAlert("Nuovo Progetto", "Crea un nuovo progetto", "OK");
        }

        private async void OnProfileButtonClicked(object sender, EventArgs e)
        {
            // Logica per visualizzare il profilo
            await DisplayAlert("Profilo", "Visualizza il profilo", "OK");
        }

        private async void OnProjectSelected(object sender, EventArgs e)
        {
            await DisplayAlert("Progetto", "Visualizza il progetto", "OK");
        }
    }
}