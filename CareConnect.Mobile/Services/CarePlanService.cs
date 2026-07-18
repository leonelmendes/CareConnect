using System.Net.Http.Json;
using System.Text.Json;
using CareConnect.Shared.Models;

namespace CareConnect.Mobile.Services;

public class CarePlanService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public CarePlanService(HttpClient httpClient, AuthService authService)
    {
        // Proteção extra: Se a injeção falhar, ele cria um novo HttpClient
        _httpClient = httpClient ?? new HttpClient(); 
        _authService = authService;
    }

    private async Task ConfigurarTokenAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<CarePlan?> CreatePlanAsync(CarePlan novoPlano)
    {
        try
        {
            await ConfigurarTokenAsync();
            
            // 1. TESTE DE SERIALIZAÇÃO: O telemóvel consegue transformar o Plano em JSON?
            // Se o TimeSpan ou o Enum derem erro, a aplicação estoira nesta exata linha.
            var opcoes = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(novoPlano, opcoes);
            System.Diagnostics.Debug.WriteLine($"[DEBUG CAREPLAN] JSON a enviar: {json}");
            
            // 2. TESTE DE URL: O BaseUrl está correto?
            string url = $"{Constants.BaseUrl}/api/CarePlans";
            System.Diagnostics.Debug.WriteLine($"[DEBUG CAREPLAN] A enviar para: {url}");

            // 3. O ENVIO
            var response = await _httpClient.PostAsJsonAsync(url, novoPlano);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CarePlan>();
            }
            
            // Se a API rejeitar (Erro 400 ou 401)
            var erroApi = await response.Content.ReadAsStringAsync();
            throw new Exception($"A API rejeitou. Status: {response.StatusCode} | Detalhe: {erroApi}");
        }
        catch (Exception ex)
        {
            // ⚠️ O NOSSO DETETIVE: Apanha o erro local e mostra no ecrã!
            throw new Exception($"ERRO LOCAL: {ex.Message}");
        }
    }

    // 1. Obter planos de um paciente
    public async Task<List<CarePlan>> GetPlansByPatientIdAsync(Guid patientId)
    {
        await ConfigurarTokenAsync();
        var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/api/careplans/patient/{patientId}");
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<CarePlan>>() ?? new List<CarePlan>();
            
        return new List<CarePlan>();
    }

    // 2. Obter um plano específico pelo ID
    public async Task<CarePlan?> GetPlanByIdAsync(Guid id)
    {
        Console.WriteLine($"[RASTREIO API] 1. GetPlanByIdAsync chamado com o Guid: {id}");
        await ConfigurarTokenAsync();
        
        var url = $"{Constants.BaseUrl}/api/careplans/{id}";
        Console.WriteLine($"[RASTREIO API] 2. URL do Pedido: {url}");

        try
        {
            var response = await _httpClient.GetAsync(url);
            Console.WriteLine($"[RASTREIO API] 3. Status Code da Resposta: {response.StatusCode}");

            // Lemos o conteúdo como string PRIMEIRO para poder imprimir na consola
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[RASTREIO API] 4. Conteúdo Recebido: {content}");

            if (response.IsSuccessStatusCode)
            {
                // Desserializamos manualmente. Usamos PropertyNameCaseInsensitive para evitar erros de maiúsculas/minúsculas no JSON
                var opcoes = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var plano = System.Text.Json.JsonSerializer.Deserialize<CarePlan>(content, opcoes);
                
                Console.WriteLine($"[RASTREIO API] 5. Desserialização com sucesso? {(plano != null ? "SIM" : "NÃO")}");
                return plano;
            }
            
            Console.WriteLine("[RASTREIO API] 5. Falha HTTP - Não é SuccessStatusCode.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RASTREIO API] EXCEÇÃO CRÍTICA: {ex.Message}");
            return null;
        }
    }

    // 3. Atualizar Plano (PUT)
    public async Task<CarePlan?> UpdatePlanAsync(CarePlan plano)
    {
        await ConfigurarTokenAsync();
        var response = await _httpClient.PutAsJsonAsync($"{Constants.BaseUrl}/api/careplans/{plano.Id}", plano);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<CarePlan>();
        throw new Exception(await response.Content.ReadAsStringAsync());
    }

    // 4. Apagar Plano (DELETE)
    public async Task<bool> DeletePlanAsync(Guid id)
    {
        await ConfigurarTokenAsync();
        var response = await _httpClient.DeleteAsync($"{Constants.BaseUrl}/api/careplans/{id}");
        return response.IsSuccessStatusCode;
    }
}