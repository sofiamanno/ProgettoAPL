using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProgettoAPL;
using ProgettoAPL.Models;
using ProgettoAPL.Services;

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

    // Metodo per la REGISTRAZIONE di un nuovo utente
    public async Task<HttpResponseMessage> RegisterUserAsync(string username, string email, string password)
    {
        var userData = new
        {
            Username = username,
            Email = email,
            Password = password
        };

        return await PostAsync("register", userData); // Modifica l'endpoint
    }

    // Funzione per verificare se l'username è già presente nel database
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

    // Metodo per il LOGIN di un utente
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
                await App.Current.MainPage.DisplayAlert("Login riuscito", "Cookie di sessione impostato correttamente.", "OK");
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

    // Metodo per ottenere i cookie di sessione
    public List<Cookie> GetSessionCookies()
    {
        return SessionManager.GetSessionCookies();
    }

    // Metodo per ottenere il profilo utente
    public async Task<Utente> GetProfileAsync()
    {
        var response = await _httpClient.GetAsync("profile"); // Modifica l'endpoint
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Utente>();
    }

    // Metodo per aggiornare il profilo utente
    public async Task<HttpResponseMessage> UpdateProfileAsync(Utente updatedProfile)
    {
        return await PutAsync("profile", updatedProfile); // Modifica l'endpoint
    }

    // Metodo per il LOGOUT di un utente
    public async Task LogoutUserAsync()
    {
        var response = await _httpClient.GetAsync("logout"); // Assicurati che l'endpoint sia corretto
        response.EnsureSuccessStatusCode();
    }

    // Metodo per ottenere la lista dei progetti
    public async Task<List<Progetto>> GetProjectsAsync()
    {
        var response = await _httpClient.GetAsync("projects"); // Modifica l'endpoint
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Progetto>>();
    }

    // Metodo per creare un nuovo progetto
    public async Task<HttpResponseMessage> CreateProjectAsync(Progetto newProject)
    {
        var response = await PostAsync("project", newProject); // Modifica l'endpoint
        response.EnsureSuccessStatusCode();
        return response;
    }

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
