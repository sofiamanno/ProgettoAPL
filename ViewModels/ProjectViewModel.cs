using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProgettoAPL.Models;
using ProgettoAPL.Services;
using ProgettoAPL.Views;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using System.Collections.ObjectModel;

namespace ProgettoAPL.ViewModels
{
    public class ProjectViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private Utente _currentUser;
        private string _projectDescription;
        private string _projectAuthor;
        private int _taskCount;
        private int _completedTaskCount;
        private ObservableCollection<TaskDisplayModel> _tasks;
        private string _newTaskDescription;
        private string _newTaskComments;
        private bool _newTaskCompleted;
        private bool _isCreatingTask;
        private ObservableCollection<Utente> _users;
        private Utente _assignedUser;

        public bool HasTasks => Tasks != null && Tasks.Count > 0;
        public bool HasNoTasks => !HasTasks;
        public int ProjectId { get; set; }

        public string ProjectDescription
        {
            get => _projectDescription;
            set => SetProperty(ref _projectDescription, value);
        }

        public string ProjectAuthor
        {
            get => _projectAuthor;
            set => SetProperty(ref _projectAuthor, value);
        }

        public int TaskCount
        {
            get => _taskCount;
            set => SetProperty(ref _taskCount, value);
        }

        public int CompletedTaskCount
        {
            get => _completedTaskCount;
            set => SetProperty(ref _completedTaskCount, value);
        }

        public ObservableCollection<TaskDisplayModel> Tasks
        {
            get => _tasks;
            set
            {
                SetProperty(ref _tasks, value);
                OnPropertyChanged(nameof(HasTasks));
                OnPropertyChanged(nameof(HasNoTasks));
            }
        }

        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set => SetProperty(ref _newTaskDescription, value);
        }

        public string NewTaskComments
        {
            get => _newTaskComments;
            set => SetProperty(ref _newTaskComments, value);
        }

        public bool NewTaskCompleted
        {
            get => _newTaskCompleted;
            set => SetProperty(ref _newTaskCompleted, value);
        }

        public bool IsCreatingTask
        {
            get => _isCreatingTask;
            set => SetProperty(ref _isCreatingTask, value);
        }

        public ObservableCollection<Utente> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public Utente AssignedUser
        {
            get => _assignedUser;
            set => SetProperty(ref _assignedUser, value);
        }

        public class TaskDisplayModel
        {
            public int Id { get; set; }
            public string Descrizione { get; set; }
            public bool Completato { get; set; }
            public string Commenti { get; set; }
            public int AutoreID { get; set; }
            public int IncaricatoID { get; set; }
            public bool CanDelete { get; set; }
        }

        public ICommand LogoutCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand NewTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand CreateTaskCommand { get; }
        public ICommand TaskSelectedCommand { get; }

        public ProjectViewModel()
        {
            _apiService = App.ApiServiceInstance;
            Tasks = new ObservableCollection<TaskDisplayModel>();
            Users = new ObservableCollection<Utente>();

            LogoutCommand = new Command(async () => await ExecuteWithExceptionHandling(OnLogout));
            ProfileCommand = new Command(async () => await ExecuteWithExceptionHandling(OnProfile));
            CreateTaskCommand = new Command(async () => await ExecuteWithExceptionHandling(OnCreateTask));
            DeleteTaskCommand = new Command<TaskDisplayModel>(async (task) => await ExecuteWithExceptionHandling(() => OnDeleteTask(task)));
            NewTaskCommand = new Command(OnNewTask);
            TaskSelectedCommand = new Command<TaskDisplayModel>(async (task) => await ExecuteWithExceptionHandling(() => OnTaskSelected(task)));

            LoadUserProfile();
            LoadUsers();
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

        private async void LoadUsers()
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var users = await _apiService.GetUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            });
        }

        public async Task LoadProjectDetails(int projectId)
        {
            ProjectId = projectId; // Imposta l'ID del progetto corrente

            await ExecuteWithExceptionHandling(async () =>
            {
                var sessionCookie = GetSessionCookie();
                if (sessionCookie != null)
                {
                    AddSessionCookieToHeaders(sessionCookie);

                    var project = await _apiService.GetProjectByIdAsync(projectId);
                    ProjectDescription = project.Descrizione;
                    var author = await _apiService.GetAuthorByIdAsync(project.AutoreID);
                    ProjectAuthor = author.Username;

                    var (totalTasks, completedTasks) = await _apiService.GetTaskCountsAsync(projectId);
                    TaskCount = totalTasks;
                    CompletedTaskCount = completedTasks;

                    var tasks = await _apiService.GetTasksByProjectIdAsync(projectId);
                    Tasks.Clear();
                    if (tasks != null && tasks.Count > 0)
                    {
                        foreach (var task in tasks)
                        {
                            try
                            {
                                Tasks.Add(new TaskDisplayModel
                                {
                                    Id = task.ID,
                                    Descrizione = task.Descrizione,
                                    Completato = task.Completato,
                                    Commenti = task.Commenti,
                                    AutoreID = task.AutoreID,
                                    IncaricatoID = task.IncaricatoID,
                                    CanDelete = task.AutoreID == _currentUser.Id
                                });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Errore durante il caricamento del task {task.Descrizione}: {ex.Message}");
                                await ShowAlert("Errore", $"Errore durante il caricamento del task {task.Descrizione}: {ex.Message}");
                            }
                        }
                        OnPropertyChanged(nameof(HasTasks));
                        OnPropertyChanged(nameof(HasNoTasks));
                    }
                    else
                    {
                        Tasks.Clear();
                        OnPropertyChanged(nameof(HasTasks));
                        OnPropertyChanged(nameof(HasNoTasks));
                    }
                }
                else
                {
                    await ShowAlert("Errore", "Nessun cookie di sessione trovato.");
                }
            });
        }

        private void OnNewTask()
        {
            IsCreatingTask = true;
        }

        private async Task OnCreateTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskDescription))
            {
                await ShowAlert("Errore", "La descrizione del task non può essere vuota.");
                return;
            }

            if (AssignedUser == null)
            {
                await ShowAlert("Errore", "Devi selezionare un utente incaricato.");
                return;
            }

            var newTask = new Compito
            {
                Descrizione = NewTaskDescription,
                Commenti = NewTaskComments,
                //Completato = NewTaskCompleted,
                AutoreID = _currentUser.Id,
                ProgettoID = ProjectId,
                IncaricatoID = AssignedUser.Id
            };

            await ExecuteWithExceptionHandling(async () =>
            {
                var createdTask = await _apiService.CreateTaskAsync(newTask);
                if (createdTask != null)
                {
                    Debug.WriteLine("Task Creato: " + createdTask.Descrizione);

                    Tasks.Add(new TaskDisplayModel
                    {
                        Id = createdTask.ID,
                        Descrizione = createdTask.Descrizione,
                        Completato = createdTask.Completato,
                        Commenti = createdTask.Commenti,
                        AutoreID = createdTask.AutoreID,
                        IncaricatoID = createdTask.IncaricatoID,
                        CanDelete = createdTask.AutoreID == _currentUser.Id
                    });

                    // Aggiorna i valori di TaskCount e CompletedTaskCount
                    TaskCount++;
                    if (createdTask.Completato)
                    {
                        CompletedTaskCount++;
                    }

                    NewTaskDescription = string.Empty;
                    NewTaskComments = string.Empty;
                    NewTaskCompleted = false;
                    IsCreatingTask = false;

                    // Aggiorna HasNoTasks
                    OnPropertyChanged(nameof(HasTasks));
                    OnPropertyChanged(nameof(HasNoTasks));
                }
                else
                {
                    await ShowAlert("Errore", "Errore durante la creazione del task.");
                }
            });
        }

        private async Task OnTaskSelected(TaskDisplayModel task)
        {
            if (task == null)
                return;

            var taskPage = new TaskPage(task.Id);
            await Application.Current.MainPage.Navigation.PushAsync(taskPage);
        }

        private async Task OnDeleteTask(TaskDisplayModel task)
        {
            bool isConfirmed = await Application.Current.MainPage.DisplayAlert(
                "Conferma Eliminazione",
                "Sei sicuro di voler eliminare questo task?",
                "Sì",
                "No"
            );

            if (isConfirmed)
            {
                await _apiService.DeleteTaskAsync(task.Id);
                Tasks.Remove(task);

                // Aggiorna i valori di TaskCount e CompletedTaskCount
                TaskCount--;
                if (task.Completato)
                {
                    CompletedTaskCount--;
                }

                // Aggiorna HasNoTasks
                OnPropertyChanged(nameof(HasTasks));
                OnPropertyChanged(nameof(HasNoTasks));
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
