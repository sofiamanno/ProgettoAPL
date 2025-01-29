using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.Services;
using ProgettoAPL.Views;
using System.Diagnostics;
using Newtonsoft.Json;


namespace ProgettoAPL.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private string _newProjectDescription;
        private bool _isCreatingProject;
        private bool _hasNoProjects;
        private readonly ApiService _apiService;
        private Utente _currentUser;

        public string NewProjectDescription
        {
            get => _newProjectDescription;
            set => SetProperty(ref _newProjectDescription, value);
        }

        public bool IsCreatingProject
        {
            get => _isCreatingProject;
            set => SetProperty(ref _isCreatingProject, value);
        }

        public bool HasNoProjects
        {
            get => _hasNoProjects;
            set => SetProperty(ref _hasNoProjects, value);
        }

        public class ProgettoDisplayModel
        {
            public int ID { get; set; } // Add this line
            public string Descrizione { get; set; }
            public int AutoreID { get; set; }
            public string AutoreUsername { get; set; }
            public bool CanDelete { get; set; }
        }



        public ObservableCollection<ProgettoDisplayModel> Progetti { get; set; }

        public ICommand LogoutCommand { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand CreateProjectCommand { get; }
        public ICommand ProjectSelectedCommand { get; }
        public ICommand DeleteProjectCommand { get; }

        public HomeViewModel()
        {
            _apiService = App.ApiServiceInstance;
            Progetti = new ObservableCollection<ProgettoDisplayModel>();

            LogoutCommand = new Command(async () => await ExecuteWithExceptionHandling(OnLogout));
            NewProjectCommand = new Command(OnNewProject);
            ProfileCommand = new Command(async () => await ExecuteWithExceptionHandling(OnProfile));
            CreateProjectCommand = new Command(async () => await ExecuteWithExceptionHandling(OnCreateProject));
            ProjectSelectedCommand = new Command<ProgettoDisplayModel>(async (project) => await ExecuteWithExceptionHandling(() => OnProjectSelected(project)));
            DeleteProjectCommand = new Command<ProgettoDisplayModel>(async (project) => await ExecuteWithExceptionHandling(() => OnDeleteProject(project)));


            LoadUserProfile();
            LoadProjects();
        }

        private async void LoadUserProfile()
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var sessionCookie = GetSessionCookie();
                if (sessionCookie != null)
                {
                    AddSessionCookieToHeaders(sessionCookie);
                    //var jsonResponse = await _apiService.GetAsync("profile");
                    //Debug.WriteLine("Profilo JSON Response: " + jsonResponse); // Scrive la risposta JSON nella finestra di debug
                    _currentUser = await _apiService.GetProfileAsync();
                }
                else
                {
                    await ShowAlert("Errore", "Nessun cookie di sessione trovato.");
                }
            });
        }

        private async void LoadProjects()
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var sessionCookie = GetSessionCookie();
                if (sessionCookie != null)
                {
                    AddSessionCookieToHeaders(sessionCookie);
                    
                    var projects = await _apiService.GetProjectsAsync();
                    Progetti.Clear();
                    if (projects != null && projects.Count > 0)
                    {
                        foreach (var project in projects)
                        {

                            try
                            {
                                var author = await _apiService.GetAuthorByIdAsync(project.AutoreID);

                                Progetti.Add(new ProgettoDisplayModel
                                {
                                    ID = project.ID, 
                                    Descrizione = project.Descrizione,
                                    AutoreID = project.AutoreID,
                                    AutoreUsername = author.Username,
                                    CanDelete = project.AutoreID == _currentUser.Id // Imposta CanDelete

                                });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Errore durante il caricamento del progetto {project.Descrizione}: {ex.Message}");
                                await ShowAlert("Errore", $"Errore durante il caricamento del progetto {project.Descrizione}: {ex.Message}");
                            }
                        }
                        HasNoProjects = false;
                    }
                    else
                    {
                        HasNoProjects = true;
                    }
                }
                else
                {
                    await ShowAlert("Errore", "Nessun cookie di sessione trovato.");
                }
            });
        }

        private void OnNewProject()
        {
            IsCreatingProject = true;
        }

        private async Task OnCreateProject()
        {
            if (string.IsNullOrWhiteSpace(NewProjectDescription))
            {
                await ShowAlert("Errore", "La descrizione del progetto non può essere vuota.");
                return;
            }

            var newProject = new Progetto
            {
                Descrizione = NewProjectDescription,
                AutoreID = _currentUser.Id,

            };

            await ExecuteWithExceptionHandling(async () =>
            {
                var createdProject = await _apiService.CreateProjectAsync(newProject);
                Debug.WriteLine("Progetto Creato: " + createdProject.Descrizione);

                // Ottieni le informazioni sull'autore utilizzando l'AutoreID
                var author = await _apiService.GetAuthorByIdAsync(createdProject.AutoreID);

                Progetti.Add(new ProgettoDisplayModel
                {
                    ID = createdProject.ID,
                    Descrizione = createdProject.Descrizione,
                    AutoreID = createdProject.AutoreID,
                    AutoreUsername = author.Username,
                    CanDelete = true
                });

                Debug.WriteLine("ho aggiunto il progetto alla lista ");
                NewProjectDescription = string.Empty;
                IsCreatingProject = false;
                HasNoProjects = false;
                await ShowAlert("Successo", "Progetto creato con successo!");
            });
        }

        private async Task OnProjectSelected(ProgettoDisplayModel project)
        {
            var projectPage = new ProjectPage(project.ID);
            await Application.Current.MainPage.Navigation.PushAsync(projectPage);
        }





        private async Task OnDeleteProject(ProgettoDisplayModel project)
        {
            bool conferma = await Application.Current.MainPage.DisplayAlert("Conferma", "Sei sicuro di voler eliminare questo progetto?", "Sì", "No");
            if (conferma)
            {
                await ExecuteWithExceptionHandling(async () =>
                {
                    try
                    {
                        await _apiService.DeleteProjectAsync(project.ID);
                        Progetti.Remove(project);
                        if (Progetti.Count == 0)
                        {
                            HasNoProjects = true;
                        }
                        await ShowAlert("Successo", "Progetto eliminato con successo!");
                    }
                    catch (HttpRequestException ex)
                    {
                        await ShowAlert("Errore", $"Errore durante la richiesta: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        await ShowAlert("Errore", $"Errore imprevisto: {ex.Message}");
                    }
                });
            }
        }


        private async Task OnLogout()
        {
            bool conferma = await Application.Current.MainPage.DisplayAlert("Logout", "Sei sicuro di voler uscire?", "Sì", "No");
            if (conferma)
            {
                await ExecuteWithExceptionHandling(async () =>
                {
                    await _apiService.LogoutUserAsync();
                    await Application.Current.MainPage.Navigation.PushAsync(new LoginPage());
                });
            }
        }

        private async Task OnProfile()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new GestioneProfiloPage());
        }

        private async Task ExecuteWithExceptionHandling(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (HttpRequestException ex)
            {
                await ShowAlert("Errore", $"Errore durante la richiesta: {ex.Message}");
            }
            catch (Exception ex)
            {
                await ShowAlert("Errore", $"Errore imprevisto: {ex.Message}");
            }
        }

        private Cookie GetSessionCookie()
        {
            var cookies = SessionManager.GetSessionCookies();
            return cookies.FirstOrDefault(c => c.Name == "session-name");
        }

        private void AddSessionCookieToHeaders(Cookie sessionCookie)
        {
            if (_apiService.HttpClient.DefaultRequestHeaders.Contains("Cookie"))
            {
                _apiService.HttpClient.DefaultRequestHeaders.Remove("Cookie");
            }
            _apiService.HttpClient.DefaultRequestHeaders.Add("Cookie", $"{sessionCookie.Name}={sessionCookie.Value}");
        }

        private async Task ShowAlert(string title, string message)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
