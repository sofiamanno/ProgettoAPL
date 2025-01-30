using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProgettoAPL;
using ProgettoAPL.Models;
using ProgettoAPL.Services;
using ProgettoAPL.Views;




public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientHandler _httpClientHandler;

    // Costruttore che inizializza l'HttpClient
    public ApiService()
    {
        _httpClientHandler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };

        _httpClient = new HttpClient(_httpClientHandler)
        {
            BaseAddress = new Uri("http://192.168.234.231:8080/") // Modifica con il tuo URL
        };
    }

    public HttpClient HttpClient => _httpClient;


    //<<-------------------------------------- REGISRAZIONE, LOGIN E PROFILO ----------------------------->
    // REGISTRAZIONE
    public async Task<HttpResponseMessage> RegisterUserAsync(string username, string email, string password)
    {

        var userData = new Utente
        {
            Username = username,
            Email = email,
            Pwd = password
        };

        return await PostAsync("register", userData); // Modifica l'endpoint
    }

    // USERNAME LIBERO
    public async Task<bool> CheckUsernameAvailabilityAsync(string username)
    {
        var response = await _httpClient.GetAsync($"check-username?username={username}"); // Modifica l'endpoint

        if (response.IsSuccessStatusCode)
        {
            var exists = await response.Content.ReadFromJsonAsync<bool>(); // Deserializziamo il valore booleano
            return exists;
        }

        return false;
    }

    // LOGIN
    public async Task LoginUserAsync(string email, string password)
    {
        var loginData = new
        {
            Email = email,
            Password = password,
        };

        try
        {
            var response = await PostAsync("login", loginData);
            response.EnsureSuccessStatusCode();

            // Verifica i cookie di sessione
            var cookies = _httpClientHandler.CookieContainer.GetCookies(new Uri("http://192.168.234.231:8080/"));
            foreach (Cookie cookie in cookies)
            {
                System.Diagnostics.Debug.WriteLine($"Cookie: {cookie.Name} = {cookie.Value}");
            }

            // Salva i cookie di sessione
            SessionManager.SetSessionCookies(cookies.Cast<Cookie>());

            // Controlla se il cookie di sessione è presente
            var sessionCookie = cookies.FirstOrDefault(c => c.Name == "session-name");
            if (sessionCookie == null)
            {
                await App.Current.MainPage.DisplayAlert("Errore di login", "Nessun cookie di sessione trovato.", "OK");
            }
            else
            {
                //await App.Current.MainPage.DisplayAlert("Login riuscito", "Cookie di sessione impostato correttamente.", "OK");
                await Application.Current.MainPage.Navigation.PushAsync(new HomePage());

            }
        }
        catch (HttpRequestException ex)
        {
            var statusCode = ex.StatusCode.HasValue ? ex.StatusCode.Value : HttpStatusCode.InternalServerError;
            var message = $"Codice di stato: {(int)statusCode}\nMessaggio: {ex.Message}";

            // Gestisci l'eccezione e mostra un messaggio di errore
            await App.Current.MainPage.DisplayAlert("Errore di login", message, "OK");
        }
    }

    // RECUPERA PASSWORD
    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var response = await _httpClient.GetAsync($"forgot-password?email={email}");

        if (response.IsSuccessStatusCode)
        {
            var exists = await response.Content.ReadFromJsonAsync<bool>(); // Deserializziamo il valore booleano
            return exists;

        }

        return false;
    }

    // COOKIE DI SESSIONE
    public List<Cookie> GetSessionCookies()
    {
        return SessionManager.GetSessionCookies();
    }



    // OTTENERE IL PROFILO
    public async Task<Utente> GetProfileAsync()
    {
        var response = await _httpClient.GetAsync("profile"); // Modifica l'endpoint
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Utente>();
    }

    public async Task<Utente> GetAuthorByIdAsync(int autoreId)
    {
        var response = await _httpClient.GetAsync($"author/?id={autoreId}");
        response.EnsureSuccessStatusCode();
        var author = await response.Content.ReadFromJsonAsync<Utente>();
        return author;
    }


    // LISTA DI TUTTI GLI UTENTI REGISTRATI
    public async Task<List<Utente>> GetUsersAsync()
    {
        var response = await _httpClient.GetAsync("all_users"); // Modifica l'endpoint con quello corretto
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Utente>>(jsonResponse);
    }



    // AGGIORNA PROFILO
    public async Task<HttpResponseMessage> UpdateProfileAsync(Utente updatedProfile)
    {
        return await PutAsync("changepwd", updatedProfile); // Modifica l'endpoint
    }

    //LOGOUT
    public async Task LogoutUserAsync()
    {
        var response = await _httpClient.GetAsync("logout"); // Assicurati che l'endpoint sia corretto
        response.EnsureSuccessStatusCode();
    }


    //<<-------------------------------------- PROGETTI ----------------------------->
    
    //LISTA DI TUTTI I PROGETTI
    public async Task<List<Progetto>> GetProjectsAsync()
    {
        var response = await _httpClient.GetAsync("projects"); // Modifica l'endpoint
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Debug.WriteLine("Risposta JSON: " + jsonResponse);
        var projects = JsonConvert.DeserializeObject<List<Progetto>>(jsonResponse);
        if (projects == null)
        {
            throw new InvalidOperationException("La risposta non contiene una lista di progetti valida.");
        }
        return projects;
    }

    // CREA NUOVO PROGETTO
    public async Task<Progetto> CreateProjectAsync(Progetto newProject)
    {
        var response = await PostAsync("project", newProject); // Modifica l'endpoint
        response.EnsureSuccessStatusCode();
        var createdProject = await response.Content.ReadFromJsonAsync<Progetto>();
        if (createdProject == null)
        {
            throw new InvalidOperationException("La risposta non contiene un progetto valido.");
        }
        Debug.WriteLine("Progetto Creato JSON Response: " + createdProject);
        return createdProject;
    }

    //ELIMINA PROGETTO
    public async Task DeleteProjectAsync(int projectId)
    {
        var response = await _httpClient.GetAsync($"delete_project/?id={projectId}");
        response.EnsureSuccessStatusCode();
    }

    //TROVA PROGETTO CON ID
    public async Task<Progetto> GetProjectByIdAsync(int projectId)
    {
        var response = await _httpClient.GetAsync($"get_project_by_id/?id={projectId}");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Progetto>(jsonResponse);
    }

    // ---------------------------------------------------- TASK ----------------------------------------------------------
    public async Task<(int TotalTasks, int CompletedTasks)> GetTaskCountsAsync(int projectId)
    {
        var response = await _httpClient.GetAsync($"count_tasks_in_project/?id={projectId}");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var taskCounts = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonResponse);
        return (taskCounts["total_tasks"], taskCounts["completed_tasks"]);
    }



    public async Task<List<Compito>> GetTasksByProjectIdAsync(int projectId)
    {
        var response = await _httpClient.GetAsync($"tasks_by_project/?progetto_id={projectId}");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Compito>>(jsonResponse);
        
    }

    public async Task<Compito> GetTaskByIdAsync(int taskId)
    {
        var response = await _httpClient.GetAsync($"get_task_by_id/?id={taskId}");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Compito>(jsonResponse);
    }

    public async Task<Compito> CreateTaskAsync(Compito newTask)
    {
        var response = await PostAsync("task", newTask); // Modifica l'endpoint con quello corretto
        response.EnsureSuccessStatusCode();
        var createdTask = await response.Content.ReadFromJsonAsync<Compito>();
        if (createdTask == null)
        {
            throw new InvalidOperationException("La risposta non contiene un task valido.");
        }
        Debug.WriteLine("Task Creato JSON Response: " + createdTask);
        return createdTask;
    }

    public async Task<HttpResponseMessage> UpdateTaskAsync(Compito updatedTask)
    {
        var response = await _httpClient.PutAsync($"update_task/{updatedTask.ID}", new StringContent(JsonConvert.SerializeObject(updatedTask), Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        return response;
    }


    public async Task DeleteTaskAsync(int taskId)
    {
        var response = await _httpClient.DeleteAsync($"delete_task/?id={taskId}");
        response.EnsureSuccessStatusCode();
    }


    //---------------------------------------------FILE --------------------------------------------------------------
    public async Task<HttpResponseMessage> UploadFileAsync(MultipartFormDataContent content)
    {
        var response = await _httpClient.PostAsync("files", content); // Modifica l'endpoint con quello corretto
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<HttpResponseMessage> UploadCodeAsync(MultipartFormDataContent content)
    {
        var response = await _httpClient.PostAsync("code", content); // Modifica l'endpoint con quello corretto
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<List<Allegato>> GetAttachmentsByTaskIdAsync(int taskId)
    {
        Debug.WriteLine("TaskId:" + taskId);
        var response = await _httpClient.GetAsync($"file_by_task/?task_id={taskId}");

        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Allegato>>(jsonResponse);
    }
    public async Task<HttpResponseMessage> DownloadAttachmentAsync(Allegato allegato)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(allegato.Id.ToString()), "id");
        
        content.Add(new StringContent(allegato.Link), "link");
        content.Add(new StringContent(allegato.Descrizione), "descrizione");
        content.Add(new StringContent(allegato.TaskID.ToString()), "taskId");

       
        var response = await _httpClient.PostAsync("download_attachment", content); // Modifica l'endpoint con quello corretto

        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<ExecutionResponse> ExecuteTaskAsync(int taskId)

    {
        var response = await _httpClient.GetAsync($"run_code/?task_id={taskId}");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ExecutionResponse>(jsonResponse);
    }

    public class ExecutionResponse
    {
        public string Message { get; set; }
        public int ExecutionId { get; set; }
    }


    public async Task<string> ViewTaskAsync(int taskId)
    {
        var response = await _httpClient.GetAsync($"get_status_code/?task_id={taskId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> StatisticsAsync(int taskId)
    {
        var response = await _httpClient.GetAsync($"get_statistics/?task_id={taskId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }




    // <----------------------------------------------- METODI GENERICI --------------------------------------------------->
    // Metodo GET Generico: Effettua richieste GET e restituisce il contenuto della risposta come stringa
    public async Task<string> GetAsync(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode(); // Lancia un'eccezione se la risposta non è 2xx
        return await response.Content.ReadAsStringAsync();
    }

    // Metodo POST Generico: Invia un oggetto serializzato come JSON usando POST
    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return response;
    }

    // Metodo PUT Generico: Aggiorna dati esistenti con PUT.
    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return response;
    }

    // Metodo DELETE Generico: Cancella risorse con DELETE.
    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
