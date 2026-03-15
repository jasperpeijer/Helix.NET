using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace Helix.Client.Web.Services;

public class AuthService(HttpClient http, IJSRuntime js)
{
    private readonly HttpClient _http = http;
    private readonly IJSRuntime _js = js;

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("https://localhost:7096/login", new { email, password });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();

            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "authToken", result.AccessToken);
                return true;
            }
        }
        
        return false;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("https://localhost:7096/register", new { email, password });
        
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
    }

    private class LoginResult
    {
        public string TokenType { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}