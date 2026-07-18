using CareConnect.Mobile.Models;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CareConnect.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "jwt_token";
    private const string ExpirationKey = "auth_expiration";
    private const string ProfileKey = "auth_profile";
    private const string LastEmailKey = "last_logged_email";
    private const string NameKey = "user_nome";
    private const string AvatarKey = "user_avatar";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<AuthResponseDto> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginDto { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync(Constants.LoginUrl, request);

            if (response.IsSuccessStatusCode)
            {
                // ⚠️ DESSERIALIZAÇÃO FORTE: Lê o Nome, AvatarUrl, Perfil e Token do DTO partilhado!
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                
                if (authResult != null && authResult.Sucesso)
                {
                    await GuardarSessaoAsync(authResult, email);
                    return authResult;
                }
            }

            var erroResult = await LerErroDaApiAsync(response);
            return new AuthResponseDto { Sucesso = false, MensagemErro = erroResult };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto { Sucesso = false, MensagemErro = $"Erro de conexão: {ex.Message}" };
        }
    }

    // --- 2. REGISTO COM O USERSCONTROLLER ---
    public async Task<AuthResponseDto> RegistarAsync(string nome, string email, string password, string perfil)
    {
        try
        {
            // Converte a string da UI para o Enum esperado pela API
            UserRole perfilEnum = perfil == "Gestor" ? UserRole.Gestor : UserRole.Cuidador;

            var request = new UserCreateDto 
            { 
                Nome = nome, 
                Email = email, 
                PasswordHash = password, // A API encripta com BCrypt quando receber!
                Role = perfilEnum 
            };
            
            var response = await _httpClient.PostAsJsonAsync(Constants.RegisterUrl, request);

            if (response.IsSuccessStatusCode)
            {
                // ⚠️ ATUALIZADO: Desserialização para o novo contrato DTO
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResult != null && authResult.Sucesso)
                {
                    await GuardarSessaoAsync(authResult, email);
                    return authResult;
                }
            }

            var erroResult = await LerErroDaApiAsync(response);
            return new AuthResponseDto { Sucesso = false, MensagemErro = erroResult };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto { Sucesso = false, MensagemErro = $"Erro de conexão: {ex.Message}" };
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            // 1. Tenta ler primeiro do cofre seguro, usando a TUA constante TokenKey
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            
            // 2. Se não encontrar no cofre seguro, tenta ler das Preferences (fallback)
            if (string.IsNullOrEmpty(token))
            {
                token = Preferences.Default.Get(TokenKey, string.Empty);
            }

            return string.IsNullOrEmpty(token) ? null : token;
        }
        catch 
        {
            // Se o SecureStorage der erro de permissão, lê das Preferences
            var tokenFallback = Preferences.Default.Get(TokenKey, string.Empty);
            return string.IsNullOrEmpty(tokenFallback) ? null : tokenFallback;
        }
    }

    private async Task GuardarSessaoAsync(AuthResponseDto response, string emailDigitado)
    {
        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, response.Token);
        }
        catch 
        {
            Preferences.Default.Set(TokenKey, response.Token);
        }

        Preferences.Default.Set(ProfileKey, response.Perfil ?? "Gestor");
        Preferences.Default.Set(ExpirationKey, response.DataExpiracao.ToString("o"));
        
        if (!string.IsNullOrWhiteSpace(response.Nome))
        {
            Preferences.Default.Set("user_nome", response.Nome);
        }
        if (!string.IsNullOrWhiteSpace(response.AvatarUrl))
        {
            Preferences.Default.Set("user_avatar", response.AvatarUrl);
        }
        
        if (!string.IsNullOrWhiteSpace(emailDigitado))
        {
            Preferences.Default.Set(LastEmailKey, emailDigitado);
        }
    }
    public void FazerLogout()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(ProfileKey);
        Preferences.Default.Remove(ExpirationKey);
        Preferences.Default.Remove(NameKey);
        Preferences.Default.Remove(AvatarKey);
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

    private async Task<string> LerErroDaApiAsync(HttpResponseMessage response)
    {
        try
        {
            var erroJson = await response.Content.ReadAsStringAsync();
            var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // ⚠️ ATUALIZADO: Muda para AuthResponseDto aqui também!
            var erroObj = JsonSerializer.Deserialize<AuthResponseDto>(erroJson, opcoes);
            
            return erroObj?.MensagemErro ?? "Erro desconhecido no servidor.";
        }
        catch
        {
            return $"Falha ao comunicar com o servidor (Status: {response.StatusCode})";
        }
    }

    public string ObterUltimoEmail()
    {
        return Preferences.Default.Get(LastEmailKey, string.Empty);
    }

    public async Task RenovarTokenSilenciosoAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync(Constants.RefreshUrl, null);

            if (response.IsSuccessStatusCode)
            {
                // ⚠️ ATUALIZADO: Agora lê diretamente o AuthResponseDto que vem da API
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResult != null && authResult.Sucesso)
                {
                    // Substitui o token velho pelo novo no cofre do telemóvel e empurra a expiração +7 dias
                    await GuardarSessaoAsync(authResult, string.Empty);
                }
            }
        }
        catch
        {
            // Falha silenciosa propositada.
        }
    }

    public async Task<bool> UploadAvatarAsync(string caminhoArquivo, string perfil)
    {
        try
        {
            // ALARME 1: O caminho chegou vazio ou o ficheiro desapareceu?
            if (string.IsNullOrEmpty(caminhoArquivo) || !File.Exists(caminhoArquivo))
            {
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Falha no Upload (Local)", $"O ficheiro da imagem não foi encontrado no telemóvel!\nCaminho: {caminhoArquivo}", "OK");
                });
                return false;
            }

            // ALARME 2: O Token de autenticação não foi gravado?
            string token = string.Empty;
            try 
            {
                token = await SecureStorage.Default.GetAsync(TokenKey);
            } 
            catch { /* Ignora erro de permissão no emulador */ }

            if (string.IsNullOrEmpty(token))
            {
                token = Preferences.Default.Get(TokenKey, string.Empty);
            }
            
            // Se depois dos dois cofrinhos continuar vazio, aí sim dispara o alarme:
            if (string.IsNullOrEmpty(token))
            {
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Falha no Upload (Token)", "Sessão sem Token JWT! O Login automático não guardou a chave.", "OK");
                });
                return false;
            }

            // 1. Definição da pasta AWS
            var pastaS3 = perfil.ToLower() switch
            {
                "gestor" or "gestores" => "gestores",
                "cuidador" or "cuidadores" => "cuidadores",
                "utente" or "utentes" => "utentes",
                _ => "avatares"
            };

            using var stream = File.OpenRead(caminhoArquivo);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            
            var extensao = Path.GetExtension(caminhoArquivo).ToLower();
            var contentType = extensao switch { ".png" => "image/png", _ => "image/jpeg" };
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "foto", Path.GetFileName(caminhoArquivo));

            var urlEndpoint = $"{Constants.BaseUrl}/api/Users/upload-avatar?pasta={pastaS3}";
            
            using var request = new HttpRequestMessage(HttpMethod.Post, urlEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            System.Diagnostics.Debug.WriteLine($"📤 A enviar imagem para a API: {urlEndpoint}");
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"✅ SUCESSO NO UPLOAD PARA AWS S3!");
                return true;
            }
            
            // ALARME 3: A API ou a AWS S3 rejeitou o pedido!
            var erroTexto = await response.Content.ReadAsStringAsync();
            MainThread.BeginInvokeOnMainThread(async () => {
                await Application.Current!.Windows[0].Page!.DisplayAlert($"Erro na API ({response.StatusCode})", erroTexto, "OK");
            });
            return false;
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () => {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Exceção no Upload", ex.Message, "OK");
            });
            return false;
        }
    }
}