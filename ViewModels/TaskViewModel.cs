using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.Services;
using ProgettoAPL.Views;
using System.Diagnostics;
using System.Net;

namespace ProgettoAPL.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private Utente _currentUser;
        private string _taskDescription;
        private string _taskComments;
        private string _taskAuthor;
        private string _assignedUser;
        private bool _taskCompleted;

        public int TaskId { get; set; }

        public string TaskDescription
        {
            get => _taskDescription;
            set => SetProperty(ref _taskDescription, value);
        }

        public string TaskComments
        {
            get => _taskComments;
            set => SetProperty(ref _taskComments, value);
        }

        public string TaskAuthor
        {
            get => _taskAuthor;
            set => SetProperty(ref _taskAuthor, value);
        }

        public string AssignedUser
        {
            get => _assignedUser;
            set => SetProperty(ref _assignedUser, value);
        }

        public bool TaskCompleted
        {
            get => _taskCompleted;
            set
            {
                if (SetProperty(ref _taskCompleted, value))
                {
                    // Aggiorna lo stato del task nel server
                    UpdateTaskCompletionStatus();
                }
            }
        }

        public ICommand LogoutCommand { get; }
        public ICommand ProfileCommand { get; }

        public TaskViewModel()
        {
            _apiService = App.ApiServiceInstance;

            LogoutCommand = new Command(async () => await ExecuteWithExceptionHandling(OnLogout));
            ProfileCommand = new Command(async () => await ExecuteWithExceptionHandling(OnProfile));

            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var sessionCookie = GetSessionCookie();
                if (sessionCookie != null)
                {
                    AddSessionCookieToHeaders(sessionCookie);
                    _currentUser = await _apiService.GetProfileAsync();
                }
                else
                {
                    await ShowAlert("Errore", "Nessun cookie di sessione trovato.");
                }
            });
        }

        public async Task LoadTaskDetails(int taskId)
        {
            TaskId = taskId;

            await ExecuteWithExceptionHandling(async () =>
            {
                var sessionCookie = GetSessionCookie();
                if (sessionCookie != null)
                {
                    AddSessionCookieToHeaders(sessionCookie);

                    var task = await _apiService.GetTaskByIdAsync(taskId);
                    TaskDescription = task.Descrizione;
                    TaskComments = task.Commenti;
                    TaskCompleted = task.Completato;
                    var author = await _apiService.GetAuthorByIdAsync(task.AutoreID);
                    TaskAuthor = author.Username;
                    var assignedUser = await _apiService.GetAuthorByIdAsync(task.IncaricatoID);
                    AssignedUser = assignedUser.Username;
                }
                else
                {
                    await ShowAlert("Errore", "Nessun cookie di sessione trovato.");
                }
            });
        }

        private async void UpdateTaskCompletionStatus()
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var updatedTask = new Compito
                {
                    ID = TaskId,
                    Completato = TaskCompleted
                };

                await _apiService.UpdateTaskAsync(updatedTask);
            });
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
