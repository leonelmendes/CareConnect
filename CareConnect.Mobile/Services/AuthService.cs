using System.Net.Http.Json;
using System.Text.Json;
using CareConnect.Mobile.Models;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;

namespace CareConnect.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    private const string ExpirationKey = "auth_expiration";
    private const string ProfileKey = "auth_profile";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            
            var response = await _httpClient.PostAsJsonAsync(Constants.LoginUrl, request);

            if (response.IsSuccessStatusCode)
            {
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
                
                if (authResult != null && authResult.Sucesso)
                {
                    await GuardarSessaoAsync(authResult);
                    return authResult;
                }
            }

            var erroResult = await LerErroDaApiAsync(response);
            return new AuthResponse { Sucesso = false, MensagemErro = erroResult };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Sucesso = false, MensagemErro = $"Erro de conexão: {ex.Message}" };
        }
    }

    public async Task<AuthResponse> RegistarAsync(string nome, string email, string password, string perfil)
    {
        try
        {
            UserRole perfilEnum = perfil == "Gestor" ? UserRole.Gestor : UserRole.Cuidador;

            var request = new UserCreateDto 
            { 
                Nome = nome, 
                Email = email, 
                PasswordHash = password, 
                Role = perfilEnum 
            };
            
            var response = await _httpClient.PostAsJsonAsync(Constants.RegisterUrl, request);

            if (response.IsSuccessStatusCode)
            {
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
                
                if (authResult != null && authResult.Sucesso)
                {
                    await GuardarSessaoAsync(authResult);
                    return authResult;
                }
            }

            var erroResult = await LerErroDaApiAsync(response);
            return new AuthResponse { Sucesso = false, MensagemErro = erroResult };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Sucesso = false, MensagemErro = $"Erro de conexão: {ex.Message}" };
        }
    }

    private async Task GuardarSessaoAsync(AuthResponse response)
    {
        await SecureStorage.Default.SetAsync(TokenKey, response.Token);
        await SecureStorage.Default.SetAsync(ProfileKey, response.Perfil);
        Preferences.Default.Set(ExpirationKey, response.DataExpiracao.ToString("o")); 
    }

    public async Task<(bool IsValid, string Perfil)> VerificarSessaoAtivaAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        var perfil = await SecureStorage.Default.GetAsync(ProfileKey);
        var expiracaoStr = Preferences.Default.Get(ExpirationKey, string.Empty);

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiracaoStr))
            return (false, string.Empty);

        if (DateTime.TryParse(expiracaoStr, out DateTime dataExpiracao))
        {
            if (DateTime.UtcNow < dataExpiracao)
            {
                return (true, perfil ?? "Gestor");
            }
        }

        FazerLogout();
        return (false, string.Empty);
    }

    public void FazerLogout()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(ProfileKey);
        Preferences.Default.Remove(ExpirationKey);
    }

    private async Task<string> LerErroDaApiAsync(HttpResponseMessage response)
    {
        try
        {
            var erroJson = await response.Content.ReadAsStringAsync();
            
            var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var erroObj = JsonSerializer.Deserialize<AuthResponse>(erroJson, opcoes);
            
            return erroObj?.MensagemErro ?? "Erro desconhecido no servidor.";
        }
        catch
        {
            return $"Falha ao comunicar com o servidor (Status: {response.StatusCode})";
        }
    }
}