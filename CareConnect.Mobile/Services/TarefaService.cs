using System.Net.Http.Json;
using CareConnect.Shared.DTOs; 
using CareConnect.Mobile.Models; 

namespace CareConnect.Mobile.Services;

public class TarefaService
{
    private readonly HttpClient _httpClient;

    public TarefaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TarefaResumo>> ObterTarefasHojeAsync()
    {
        try
        {
            // O teu método de configurar o token se necessário
            // await ConfigurarTokenAsync(); 

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