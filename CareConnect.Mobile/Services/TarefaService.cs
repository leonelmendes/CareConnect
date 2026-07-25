using System.Net.Http.Json;
using CareConnect.Shared.DTOs; 
using CareConnect.Mobile.Models;
using System.Net.Http.Headers;

namespace CareConnect.Mobile.Services;

public class TarefaService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    public TarefaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TarefaResumo>> ObterTarefasHojeAsync()
    {
        try
        {
            // 1. Vai buscar o token guardado no login
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            
            if (!string.IsNullOrEmpty(token))
            {
                // 2. Anexa o token ao cabeçalho do pedido
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/api/Dashboard/tarefas-hoje");

            if (response.IsSuccessStatusCode)
            {
                var dtos = await response.Content.ReadFromJsonAsync<List<TarefaResumoDto>>();
                if (dtos == null) return new List<TarefaResumo>();

                return dtos.Select(dto => new TarefaResumo
                {
                    Id = dto.Id,
                    DataHora = dto.DataHora,
                    Titulo = dto.Titulo,
                    NomeUtente = dto.NomeUtente,
                    EstaConcluida = dto.Concluida
                }).ToList();
            }
            
            return new List<TarefaResumo>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao buscar tarefas: {ex.Message}");
            return new List<TarefaResumo>();
        }
    }
}