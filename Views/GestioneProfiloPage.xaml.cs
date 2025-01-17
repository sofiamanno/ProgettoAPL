using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.ViewModels;  // Namespace per il modello Utente

namespace ProgettoAPL.Views
{
    public partial class GestioneProfiloPage : ContentPage
    {
        public GestioneProfiloPage()
        {
            InitializeComponent();
            BindingContext = new GestioneProfiloViewModel();
        }

        private void OnModificaPasswordClicked(object sender, EventArgs e)
        {
            // Esegui il comando di conferma password
            ((GestioneProfiloViewModel)BindingContext).ConfermaPasswordCommand.Execute(null);

            // Svuota i campi di input
            NuovaPasswordEntry.Text = string.Empty;
            ConfermaPasswordEntry.Text = string.Empty;
        }
    }
}
