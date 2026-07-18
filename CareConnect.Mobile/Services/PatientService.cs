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
}