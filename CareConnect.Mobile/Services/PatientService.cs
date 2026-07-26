using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareConnect.Shared.Models;

namespace CareConnect.Mobile.Services;

public class PatientService
{
    private readonly HttpClient _httpClient;

    public PatientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task ConfigurarTokenAsync()
    {
        // Lê o token que guardámos no Login (Preferences ou SecureStorage)
        var token = Preferences.Default.Get("jwt_token", string.Empty);
        if (string.IsNullOrEmpty(token))
        {
            try { token = await SecureStorage.Default.GetAsync("jwt_token"); } catch { }
        }

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // 1. BUSCA TODOS OS UTENTES DO GESTOR LOGADO
    public async Task<List<Patient>> GetMyPatientsAsync()
    {
        await ConfigurarTokenAsync();
        var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/api/Patients");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Patient>>() ?? new List<Patient>();
        }
        return new List<Patient>();
    }

    // 2. CRIA UM NOVO UTENTE
    public async Task<Patient?> CreatePatientAsync(Patient novoPaciente)
    {
        try
        {
            await ConfigurarTokenAsync();
            var response = await _httpClient.PostAsJsonAsync($"{Constants.BaseUrl}/api/Patients", novoPaciente);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Patient>();
            }
            else
            {
                // ⚠️ O NOSSO DETETIVE: Lê a mensagem de erro exata que a API devolveu
                var erroJson = await response.Content.ReadAsStringAsync();
                
                // Lançamos uma exceção forçada para o ViewModel apanhar e mostrar no ecrã!
                throw new Exception($"Status {response.StatusCode} | Detalhe: {erroJson}");
            }
        }
        catch (Exception ex)
        {
            // Vai reencaminhar o erro detalhado para a interface gráfica
            System.Diagnostics.Debug.WriteLine($"EXCEÇÃO API: {ex.Message}");
            throw; 
        }
    }

    // 3. ENVIA A FOTO DO UTENTE PARA A AWS S3
    public async Task<bool> UploadPatientAvatarAsync(Guid patientId, string caminhoArquivo)
    {
        try
        {
            await ConfigurarTokenAsync();
            using var stream = File.OpenRead(caminhoArquivo);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "foto", Path.GetFileName(caminhoArquivo));

            var response = await _httpClient.PostAsync($"{Constants.BaseUrl}/api/Patients/{patientId}/upload-avatar", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro no upload do utente: {ex.Message}");
            return false;
        }
    }

    // 4. APAGA (DESATIVA) UM UTENTE
    public async Task<bool> DeletePatientAsync(Guid patientId)
    {
        try
        {
            await ConfigurarTokenAsync();
            var response = await _httpClient.DeleteAsync($"{Constants.BaseUrl}/api/Patients/{patientId}");

            // O teu back-end retorna NoContent (204) quando o DeactivateAsync funciona
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var erroJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Status {response.StatusCode} | Detalhe: {erroJson}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao apagar utente: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdatePatientAsync(Patient patient)
    {
        try
        {
            await ConfigurarTokenAsync();

            // ADICIONADO: Configuração para camelCase!
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Usa as opções na hora de converter
            var json = System.Text.Json.JsonSerializer.Serialize(patient, options);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{Constants.BaseUrl}/api/Patients/{patient.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var erroJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar (Status {response.StatusCode}): {erroJson}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exceção ao atualizar utente: {ex.Message}");
            return false;
        }
    }

    public async Task<string> UploadFotoPerfilAsync(Guid patientId, FileResult foto)
    {
        try
        {
            await ConfigurarTokenAsync();

            using var stream = await foto.OpenReadAsync();
            using var content = new MultipartFormDataContent();
            
            // CORREÇÃO 1: Mudar "file" para "foto" para coincidir com o "IFormFile foto" da tua API
            content.Add(new StreamContent(stream), "foto", foto.FileName);

            var response = await _httpClient.PostAsync($"{Constants.BaseUrl}/api/Patients/{patientId}/upload-avatar", content);

            var respostaTexto = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // CORREÇÃO 2: Ler o JSON que a tua API devolve e extrair apenas a propriedade "avatarUrl"
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(respostaTexto);
                if (jsonDoc.RootElement.TryGetProperty("avatarUrl", out var urlElement))
                {
                    return urlElement.GetString(); // Devolve APENAS o link do S3 limpo!
                }
                
                return null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ERRO API UPLOAD: {response.StatusCode} - {respostaTexto}");
                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exceção no UploadFotoPerfilAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Patient>> GetMeusPacientesAsync()
    {
        try
        {
            // O HttpClient já deve estar configurado com o JWT Token do Cuidador
            var response = await _httpClient.GetAsync("api/Patients/meus-pacientes");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Patient>>() ?? new List<Patient>();
            }
        }
        catch (Exception ex)
        {
            // Log do erro
        }

        return new List<Patient>();
    }
}