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
    private const string TokenKey = "auth_token";
    private const string ExpirationKey = "auth_expiration";
    private const string ProfileKey = "auth_profile";
    private const string LastEmailKey = "last_logged_email";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            // Usamos o LoginDto oficial do projeto partilhado
            var request = new LoginDto { Email = email, Password = password };
            
            var response = await _httpClient.PostAsJsonAsync(Constants.LoginUrl, request);

            if (response.IsSuccessStatusCode)
            {
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResult != null && authResult.Sucesso)
                {
                    await GuardarSessaoAsync(authResult, email);
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

    // --- 2. REGISTO COM O USERSCONTROLLER ---
    public async Task<AuthResponse> RegistarAsync(string nome, string email, string password, string perfil)
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
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResult != null && authResult.Sucesso)
                {
                    await GuardarSessaoAsync(authResult, email);
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

    private async Task GuardarSessaoAsync(AuthResponse response, string emailDigitado)
    {
        await SecureStorage.Default.SetAsync(TokenKey, response.Token);
        await SecureStorage.Default.SetAsync(ProfileKey, response.Perfil);
        Preferences.Default.Set(ExpirationKey, response.DataExpiracao.ToString("o"));
        
        if (!string.IsNullOrWhiteSpace(emailDigitado))
        {
            Preferences.Default.Set(LastEmailKey, emailDigitado);
        }
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
                var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
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

    public async Task<bool> UploadAvatarAsync(string caminhoArquivo)
    {
        try
        {
            // 1. Validação local: verifica se o ficheiro realmente existe no telemóvel
            if (string.IsNullOrEmpty(caminhoArquivo) || !File.Exists(caminhoArquivo))
                return false;

            // 2. Vai buscar o Token JWT ao cofre do telemóvel
            var token = await SecureStorage.Default.GetAsync("jwt_token");
            if (string.IsNullOrEmpty(token))
                return false;

            // 3. Prepara o ficheiro para envio em formato MultipartFormData
            using var stream = File.OpenRead(caminhoArquivo);
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(stream);

            // Descobre o tipo de imagem (PNG ou JPEG)
            var extensao = Path.GetExtension(caminhoArquivo).ToLower();
            var contentType = extensao switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "image/jpeg" // Fallback padrão
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // ⚠️ ATENÇÃO CRÍTICA: O primeiro parâmetro ("foto") TEM de ser rigorosamente igual 
            // ao nome da variável no teu Controller da API: UploadAvatar(IFormFile foto)
            content.Add(fileContent, "foto", Path.GetFileName(caminhoArquivo));

            // 4. Cria um pedido HTTP manual para podermos injetar o Bearer Token
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BaseUrl}/api/Auth/upload-avatar");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // 5. Envia para a API (que por sua vez mandará para a AWS S3)
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Lê a resposta da API para pegar o link da AWS e guardar nas Preferences para uso rápido
                var resultado = await response.Content.ReadFromJsonAsync<UploadAvatarResponse>();
                if (resultado != null && !string.IsNullOrEmpty(resultado.AvatarUrl))
                {
                    Preferences.Default.Set("auth_avatar", resultado.AvatarUrl);
                }
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro no upload do avatar: {ex.Message}");
            return false;
        }
    }
}