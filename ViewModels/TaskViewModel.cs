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
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Mail;
using Newtonsoft.Json;
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
        private bool _isFileSelected;
        private FileResult _fileToSend;
        private string _uploadedFile;
        private string _uploadedCode;
        private string _selectedFileName;
        private ObservableCollection<Allegato> _attachments;

        public string SelectedFileName
        {
            get => _selectedFileName;
            set => SetProperty(ref _selectedFileName, value);
        }
        public ObservableCollection<Allegato> Attachments
        {
            get => _attachments;
            set => SetProperty(ref _attachments, value);
        }
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
       

        public bool IsFileSelected
        {
            get => _isFileSelected;
            set => SetProperty(ref _isFileSelected, value);
        }

        public FileResult FileToSend
        {
            get => _fileToSend;
            set => SetProperty(ref _fileToSend, value);
        }

        public string UploadedFile
        {
            get => _uploadedFile;
            set => SetProperty(ref _uploadedFile, value);


        }

        public string UploadedCode
        {
            get => _uploadedCode;
            set => SetProperty(ref _uploadedCode, value);
        }



        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------

        public ICommand LogoutCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand UploadFileCommand { get; }

        public ICommand UploadCodeCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand ConfirmSendFileCommand { get; }
        public ICommand DownloadAttachmentCommand { get; }
        public ICommand ExecuteTaskCommand { get; }
        public ICommand ViewTaskCommand { get; }
        public ICommand StatisticsCommand { get; }
        public ICommand ViewResultsCommand { get; }


        public TaskViewModel()
        {
            _apiService = App.ApiServiceInstance;

            LogoutCommand = new Command(async () => await ExecuteWithExceptionHandling(OnLogout));
            ProfileCommand = new Command(async () => await ExecuteWithExceptionHandling(OnProfile));
            UploadFileCommand = new Command(async () => await ExecuteWithExceptionHandling(PickAndShowFileAsync));
            UploadCodeCommand = new Command(async () => await ExecuteWithExceptionHandling(PickAndShowCodeAsync));
            ConfirmSendFileCommand = new Command(async () => await ExecuteWithExceptionHandling(() => ConfirmSendFileAsync(TaskId)));
            DownloadAttachmentCommand = new Command<Allegato>(async (attachment) => await ExecuteWithExceptionHandling(() => DownloadAttachmentAsync(attachment)));
            ExecuteTaskCommand = new Command(async () => await ExecuteTaskAsync(TaskId));
            ViewTaskCommand = new Command(async () => await ViewTaskAsync(TaskId));
            StatisticsCommand = new Command(async () => await ShowStatisticsAsync(TaskId));
            ViewResultsCommand = new Command(async () => await ViewResultsAsync(TaskId));


            Attachments = new ObservableCollection<Allegato>();

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

                    await LoadAttachments(taskId);
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


        private async Task PickAndShowFileAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    // Controlla l'estensione del file
                    if (Path.GetExtension(result.FileName).Equals(".py", StringComparison.OrdinalIgnoreCase))
                    {
                        await Application.Current.MainPage.DisplayAlert("Errore", "Non è possibile caricare file con estensione .py.", "OK");
                        return;
                    }

                    FileToSend = result;
                    IsFileSelected = true;
                    UploadedFile = result.FileName;
                    UploadedCode = null; // Clear the code if a file is selected
                    SelectedFileName = result.FileName;
                    Debug.WriteLine("File selezionato: " + result.FileName);
                }
            }
            catch (Exception ex)
            {
                // Gestisci eventuali errori
                await Application.Current.MainPage.DisplayAlert("Errore", ex.Message, "OK");
            }
        }

        private async Task PickAndShowCodeAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Seleziona un file Python",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".py" } }, // Windows specific file extension
            })
                });

                if (result != null)
                {
                    FileToSend = result;
                    IsFileSelected = true;
                    UploadedCode = result.FileName;
                    UploadedFile = null; // Clear the file if a code is selected
                    SelectedFileName = result.FileName;
                    Debug.WriteLine("Codice selezionato: " + result.FileName);
                }
            }
            catch (Exception ex)
            {
                // Gestisci eventuali errori
                await Application.Current.MainPage.DisplayAlert("Errore", ex.Message, "OK");
            }
        }


        private async Task ConfirmSendFileAsync(int taskId)
        {
            if (FileToSend != null)
            {
                if (!string.IsNullOrEmpty(UploadedFile))
                {
                    await SendFileAsync(FileToSend, "file", taskId);
                }
                else if (!string.IsNullOrEmpty(UploadedCode))
                {
                    await SendFileAsync(FileToSend, "code", taskId);
                }
                IsFileSelected = false;
                FileToSend = null;
                SelectedFileName = null;
                await LoadAttachments(taskId); // Ricarica gli allegati per aggiornare la lista

            }
        }

        private async Task SendFileAsync(FileResult file, string route, int taskId)
        {
            try
            {
                using (var stream = await file.OpenReadAsync())
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(stream), "file", file.FileName);
                    content.Add(new StringContent(taskId.ToString()), "taskId");

                    HttpResponseMessage response;
                    if (route == "file")
                    {
                        response = await _apiService.UploadFileAsync(content);
                    }
                    else
                    {
                        response = await _apiService.UploadCodeAsync(content);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        await ShowAlert("Successo", "File inviato con successo.");
                    }
                    else
                    {
                        await ShowAlert("Errore", "Errore durante l'invio del file.");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Errore", $"Errore durante l'invio del file: {ex.Message}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------


        private async Task LoadAttachments(int taskId)
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var attachments = await _apiService.GetAttachmentsByTaskIdAsync(taskId);
                Attachments.Clear();
                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var attachment in attachments)
                    {
                        Attachments.Add(attachment);
                    }
                }
                else
                {
                    Debug.WriteLine("Nessun allegato trovato per il task con ID: " + taskId);
                }
            });
        }


        private async Task DownloadAttachmentAsync(Allegato attachment)
        {
            try
            {
                var response = await _apiService.DownloadAttachmentAsync(attachment);
                var stream = await response.Content.ReadAsStreamAsync();
                var filePath = Path.Combine("C:\\Users\\Sofia\\Downloads", attachment.Descrizione);

                using (var fileStream = File.Create(filePath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                await ShowAlert("Successo", $"Allegato scaricato: {filePath}");
            }
            catch (Exception ex)
            {
                await ShowAlert("Errore", $"Errore durante il download dell'allegato: {ex.Message}");
            }
        }


        private async Task ExecuteTaskAsync(int taskId)
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                Debug.WriteLine("TASKID:  " + taskId);  
                var response = await _apiService.ExecuteTaskAsync(taskId);
                await ShowAlert("Successo", $"Esecuzione del codice avviata. ID esecuzione: {response.ExecutionId}");
                await LoadAttachments(taskId);
            });
        }



        private async Task ViewTaskAsync(int taskId)
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var jsonResponse = await _apiService.ViewTaskAsync(taskId);
                var taskData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

                if (taskData != null && taskData.TryGetValue("status", out var statoEsecuzione))
                {
                    var message = $"Stato di Esecuzione: {statoEsecuzione}";
                    await ShowAlert("Stato Esecuzione", message);
                }
                else
                {
                    await ShowAlert("Errore", "Formato dello stato di esecuzione non valido.");
                }

                await LoadAttachments(taskId);
            });
        }



        private async Task ShowStatisticsAsync(int taskId)
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var jsonResponse = await _apiService.StatisticsAsync(taskId);
                var statisticsData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

                if (statisticsData != null && statisticsData.TryGetValue("errors", out var errors) && statisticsData.TryGetValue("execution_time", out var executionTime))
                {
                    var message = $"Numero di errori: {errors}\nTempo di esecuzione: {executionTime} secondi";
                    await ShowAlert("Statistiche", message);
                }
                else
                {
                    await ShowAlert("Errore", "Formato delle statistiche non valido.");
                }

                await LoadAttachments(taskId);
            });
        }

        private async Task ViewResultsAsync(int taskId)
        {
            await ExecuteWithExceptionHandling(async () =>
            {
                var jsonResponse = await _apiService.ResultsAsync(taskId);
                var resultsData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

                if (resultsData != null && resultsData.TryGetValue("output", out var output))
                {
                    string message;
                    if (resultsData.TryGetValue("created_files", out var files))
                    {
                        var filesList = JsonConvert.DeserializeObject<List<string>>(files.ToString());
                        var filesMessage = string.Join("\n", filesList);
                        message = $"File creati:\n{filesMessage}\n\nOutput:\n{output}";
                    }
                    else
                    {
                        message = $"Output:\n{output}";
                    }
                    await ShowAlert("Risultati", message);
                }
                else
                {
                    await ShowAlert("Errore", "Formato dei risultati non valido.");
                }

                await LoadAttachments(taskId);
            });
        }






        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------

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
