using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProgettoAPL.ViewModels;

namespace ProgettoAPL.Views
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
            BindingContext = new ForgotPasswordViewModel();
        }

        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            ((ForgotPasswordViewModel)BindingContext).SendCommand.Execute(null);
            await Application.Current.MainPage.DisplayAlert("Recupero Password", "Email di recupero inviata!", "OK");
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}

