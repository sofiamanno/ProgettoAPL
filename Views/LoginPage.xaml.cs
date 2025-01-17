using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;
using ProgettoAPL.ViewModels;

namespace ProgettoAPL.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
        }


        private async void OnRegisterLabelTapped(object sender, EventArgs e)
        {
            // Naviga alla pagina di registrazione
            await Navigation.PushAsync(new RegisterPage());
        }
        private async void OnForgotPasswordLabelTapped(object sender, EventArgs e)
        {
            // Naviga alla pagina di recupero password
            await Navigation.PushAsync(new ForgotPasswordPage());
        }

    }
}