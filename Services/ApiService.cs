using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProgettoAPL.Models;


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
            CookieContainer = new System.Net.CookieContainer()
        };

        _httpClient = new HttpClient(_httpClientHandler)
        {
            BaseAddress = new Uri("https://localhost:8080/") // Modifica con il tuo URL
        };
    }

    // Metodo per la REGISTRAZIONE di un nuovo utente
    public async Task<HttpResponseMessage> RegisterUserAsync(string username, string email, string password)
    {
        var userData = new
        {
            Username = username,
            Email = email,
            Password = password
        };

        return await PostAsync("auth/register", userData);
    }

    // Funzione per verificare se l'username è già presente nel database
    public async Task<bool> CheckUsernameAvailabilityAsync(string username)
    {
        var response = await _httpClient.GetAsync($"auth/check-username?username={username}");

        if (response.IsSuccessStatusCode)
        {
            var exists = await response.Content.ReadFromJsonAsync<bool>(); // Deserializziamo il valore booleano
            return exists;
        }

        return false;
    }

    // Metodo per il LOGIN di un utente
    public async Task<string> LoginUserAsync(string email, string password)
    {
        var loginData = new
        {
            Email = email,
            Password = password
        };

        var response = await PostAsync("auth/login", loginData);
        response.EnsureSuccessStatusCode();

        // Supponiamo che il backend restituisca un oggetto con l'ID utente
        var user = await response.Content.ReadFromJsonAsync<Utente>();
        return user?.Id; // Assicurati che l'oggetto Utente abbia una proprietà Id
    }

    // Metodo per ottenere il profilo utente
    public async Task<Utente> GetProfileAsync(string userId)
    {
        var response = await _httpClient.GetAsync($"profile/{userId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Utente>();
    }

    // Metodo per aggiornare il profilo utente
    public async Task<HttpResponseMessage> UpdateProfileAsync(string userId, Utente updatedProfile)
    {
        return await PutAsync($"profile/{userId}", updatedProfile);
    }



    //-----------------------------------------------------------------------------------------------------
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
