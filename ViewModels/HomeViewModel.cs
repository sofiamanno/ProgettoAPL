using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.Services;
using ProgettoAPL.Views;


namespace ProgettoAPL.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private string _newProjectDescription;
        private bool _isCreatingProject;
        private bool _hasProjects;
        private readonly ApiService _apiService;
        private Utente _currentUser;

        public string NewProjectDescription
        {
            get => _newProjectDescription;
            set
            {
                if (_newProjectDescription != value)
                {
                    _newProjectDescription = value;
                    OnPropertyChanged(nameof(NewProjectDescription));
                }
            }
        }

        public bool IsCreatingProject
        {
            get => _isCreatingProject;
            set
            {
                if (_isCreatingProject != value)
                {
                    _isCreatingProject = value;
                    OnPropertyChanged(nameof(IsCreatingProject));
                }
            }
        }

        public bool HasProjects
        {
            get => _hasProjects;
            set
            {
                if (_hasProjects != value)
                {
                    _hasProjects = value;
                    OnPropertyChanged(nameof(HasProjects));
                }
            }
        }

        public class ProgettoDisplayModel
        {
            public string Descrizione { get; set; }
            public string AutoreNome { get; set; }
        }

        public ObservableCollection<ProgettoDisplayModel> Progetti { get; set; }

        // Comandi statici per ora
        public ICommand LogoutCommand { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CreateProjectCommand { get; }
        public ICommand ProjectSelectedCommand { get; }

        public HomeViewModel()
        {
            _apiService = App.ApiServiceInstance;

            Progetti = new ObservableCollection<ProgettoDisplayModel>();

            // Comandi collegati a metodi statici per ora
            LogoutCommand = new Command(async () => await OnLogout());
            NewProjectCommand = new Command(OnNewProject);
            ProfileCommand = new Command(async () => await OnProfile());
            CreateProjectCommand = new Command(async () => await OnCreateProject());
            ProjectSelectedCommand = new Command<ProgettoDisplayModel>(async (project) => await OnProjectSelected(project));

            // Carica i progetti e il profilo utente all'avvio
            LoadUserProfile();
            LoadProjects();
        }

        private async void LoadUserProfile()
        {
            try
            {
                // Recupera i cookie di sessione
                var cookies = SessionManager.GetSessionCookies();
                var sessionCookie = cookies.FirstOrDefault(c => c.Name == "session-name");

                if (sessionCookie != null)
                {
                    // Rimuovi l'intestazione se esiste già
                    if (_apiService.HttpClient.DefaultRequestHeaders.Contains("Cookie"))
                    {
                        _apiService.HttpClient.DefaultRequestHeaders.Remove("Cookie");
                    }

                    // Aggiungi il cookie di sessione alle intestazioni delle richieste
                    _apiService.HttpClient.DefaultRequestHeaders.Add("Cookie", $"{sessionCookie.Name}={sessionCookie.Value}");

                    // Chiama la rotta del server che richiede autenticazione
                    _currentUser = await _apiService.GetProfileAsync();
                }
                else
                {
                    Console.WriteLine("Nessun cookie di sessione trovato.");
                    await Application.Current.MainPage.DisplayAlert("Errore", "Nessun cookie di sessione trovato.", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Errore durante il caricamento del profilo utente: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Errore", $"Errore durante il caricamento del profilo utente: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore imprevisto: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Errore", $"Errore imprevisto: {ex.Message}", "OK");
            }
        }

        private async void LoadProjects()
        {
            try
            {
                // Recupera i cookie di sessione
                var cookies = SessionManager.GetSessionCookies();
                var sessionCookie = cookies.FirstOrDefault(c => c.Name == "session-name");

                if (sessionCookie != null)
                {
                    // Rimuovi l'intestazione se esiste già
                    if (_apiService.HttpClient.DefaultRequestHeaders.Contains("Cookie"))
                    {
                        _apiService.HttpClient.DefaultRequestHeaders.Remove("Cookie");
                    }

                    // Aggiungi il cookie di sessione alle intestazioni delle richieste
                    _apiService.HttpClient.DefaultRequestHeaders.Add("Cookie", $"{sessionCookie.Name}={sessionCookie.Value}");

                    // Chiama le rotte del server che richiedono autenticazione
                    var projects = await _apiService.GetProjectsAsync();
                    Progetti.Clear();
                    foreach (var project in projects)
                    {
                        Progetti.Add(new ProgettoDisplayModel { Descrizione = project.Descrizione, AutoreNome = project.Autore.Nome });
                    }
                    HasProjects = Progetti.Count > 0;
                }
                else
                {
                    Console.WriteLine("Nessun cookie di sessione trovato.");
                    await Application.Current.MainPage.DisplayAlert("Errore", "Nessun cookie di sessione trovato.", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Errore durante il caricamento dei progetti: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Errore", $"Errore durante il caricamento dei progetti: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore imprevisto: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Errore", $"Errore imprevisto: {ex.Message}", "OK");
            }
        }

        private void OnNewProject()
        {
            IsCreatingProject = true;
        }

        private async Task OnCreateProject()
        {
            if (string.IsNullOrWhiteSpace(NewProjectDescription))
            {
                await Application.Current.MainPage.DisplayAlert("Errore", "La descrizione del progetto non può essere vuota.", "OK");
                return;
            }

            var newProject = new Progetto
            {
                Descrizione = NewProjectDescription,
                Autore = _currentUser // Utilizza l'oggetto Utente per l'autore
            };

            try
            {
                await _apiService.CreateProjectAsync(newProject);
                Progetti.Add(new ProgettoDisplayModel { Descrizione = newProject.Descrizione, AutoreNome = newProject.Autore.Nome });
                NewProjectDescription = string.Empty;
                IsCreatingProject = false;
                HasProjects = Progetti.Count > 0;
                await Application.Current.MainPage.DisplayAlert("Successo", "Progetto creato con successo!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Errore", $"Si è verificato un errore: {ex.Message}", "OK");
            }
        }

        private async Task OnProjectSelected(ProgettoDisplayModel project)
        {
            // Naviga alla pagina di dettaglio del progetto
            // await Application.Current.MainPage.Navigation.PushAsync(new ProgettoView(project));
        }

        // Metodi statici con commenti per future chiamate al server
        private async Task OnLogout()
        {
            bool conferma = await Application.Current.MainPage.DisplayAlert(
                  "Logout", "Sei sicuro di voler uscire?", "Sì", "No");

            if (conferma)
            {
                try
                {
                    await _apiService.LogoutUserAsync();
                    // Reset eventuali dati utente (se presenti)
                    // Application.Current.Properties.Clear();
                    // await Application.Current.SavePropertiesAsync();

                    // Navigazione alla pagina di Login
                    await Application.Current.MainPage.Navigation.PushAsync(new LoginPage());
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Errore", $"Si è verificato un errore: {ex.Message}", "OK");
                }
            }
        }

        private async Task OnProfile()
        {
            // Naviga alla pagina di gestione del profilo
            await Application.Current.MainPage.Navigation.PushAsync(new GestioneProfiloPage());
        }

        // Implementazione INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
