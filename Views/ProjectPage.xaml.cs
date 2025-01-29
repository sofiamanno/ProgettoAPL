using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProgettoAPL.ViewModels;

namespace ProgettoAPL.Views
{
    public partial class ProjectPage : ContentPage
    {
        private int _projectId;
        public ProjectPage(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            var viewModel = (ProjectViewModel)BindingContext;
            viewModel.LoadProjectDetails(projectId);
        }

        private async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            // Logica di logout
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnProfileButtonClicked(object sender, EventArgs e)
        {
            // Logica per visualizzare il profilo
            //await DisplayAlert("Profilo", "Visualizza il profilo", "OK");
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            var viewModel = (ProjectViewModel)BindingContext;
            viewModel.LoadProjectDetails(_projectId);
        }
    }
}
