using System.Net.Http.Json;
using System.Net.Http.Headers;
using CareConnect.Shared.DTOs;
using CareConnect.Mobile.Models;

namespace CareConnect.Mobile.Services;

public class TarefaService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";

    public TarefaService(HttpClient httpClient)
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

    // 1. Mudámos o nome para receber a data específica!
    public async Task<List<TarefaResumo>> ObterTarefasPorDataAsync(DateTime data)
    {
        try
        {
            // 1. GARANTIR QUE O TOKEN É CARREGADO (MUITO IMPORTANTE!)
            await ConfigurarTokenAsync();

            string dataFormatada = data.ToString("yyyy-MM-dd");
            // Coloquei 'TaskLogs' com T e L maiúsculos para garantir correspondência exata na rota
            var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/api/TaskLogs/dia/{dataFormatada}");

            // 2. LÊ O TEXTO DA RESPOSTA PARA PODERMOS VER O QUE A API DEVOLVEU
            var rawJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Se houver erro de rota ou autenticação, isto vai rebentar na tua cara com o motivo!
                await Application.Current.MainPage.DisplayAlert("Erro Crítico API", $"Status: {response.StatusCode}\nMotivo: {rawJson}", "OK");
                return new List<TarefaResumo>();
            }
            /*
            if (rawJson == "[]")
            {
                // Se a API devolver vazio, o telemóvel vai avisar-te que a culpa é do backend
                await Application.Current.MainPage.DisplayAlert("Aviso", "A API devolveu uma lista vazia []. Nenhuma tarefa encontrada para este utilizador.", "OK");
                return new List<TarefaResumo>();
            }
            */
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dtos = System.Text.Json.JsonSerializer.Deserialize<List<TarefaResumoDto>>(rawJson, options);

            if (dtos == null) return new List<TarefaResumo>();

            return dtos.Select(dto => new TarefaResumo
            {
                Id = dto.Id,
                DataHora = dto.DataHora,
                Titulo = dto.Titulo,
                Categoria = dto.Categoria,
                NomeUtente = dto.NomeUtente,
                EstaConcluida = dto.Concluida,
                AvatarUtente = dto.AvatarUtente,
                TimestampExecucao = dto.TimestampExecucao,
                Notas = dto.Notas,
                IsAdHoc = dto.IsAdHoc
            }).ToList();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Exceção App", ex.Message, "OK");
            return new List<TarefaResumo>();
        }
    }

    public async Task<bool> RegistarAdHocAsync(RegistoAdHocDto dto)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Mudámos o URL para o TaskLogsController
            var response = await _httpClient.PostAsJsonAsync($"{Constants.BaseUrl}/api/tasklogs/adhoc", dto);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao registar ad-hoc: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AtualizarEstadoTarefaAsync(Guid tarefaId, int novoStatus, string notas = "")
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Criamos um objeto anónimo que corresponde ao 'UpdateStatusDto' que tens no Controller da API
            var payload = new
            {
                Status = novoStatus,
                Notas = notas
            };

            // Fazemos o pedido PATCH
            var response = await _httpClient.PatchAsJsonAsync($"{Constants.BaseUrl}/api/tasklogs/{tarefaId}/status", payload);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao atualizar tarefa: {ex.Message}");
            return false;
        }
    }

    public async Task<List<UtenteResumo>> ObterMeusUtentesAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/api/patients/meus-utentes");

            if (response.IsSuccessStatusCode)
            {
                // MAGIA AQUI: Obriga o C# a ignorar as diferenças de maiúsculas/minúsculas do JSON
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var utentes = await response.Content.ReadFromJsonAsync<List<UtenteResumo>>(options);
                return utentes ?? new List<UtenteResumo>();
            }
            return new List<UtenteResumo>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar utentes: {ex.Message}");
            return new List<UtenteResumo>();
        }
    }

    public async Task<string> UploadFotoAdHocAsync(string caminhoArquivo)
    {
        try
        {
            await ConfigurarTokenAsync();

            using var stream = File.OpenRead(caminhoArquivo);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "foto", Path.GetFileName(caminhoArquivo));

            // Chama a rota que acabaste de criar no backend
            var response = await _httpClient.PostAsync($"{Constants.BaseUrl}/api/TaskLogs/upload-foto", content);
            var respostaTexto = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Extrai o link limpo do JSON que a tua API devolveu
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(respostaTexto);
                if (jsonDoc.RootElement.TryGetProperty("fotoUrl", out var urlElement))
                {
                    return urlElement.GetString();
                }
                return string.Empty;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"=== ERRO UPLOAD AWS ===");
                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode} - {respostaTexto}");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== EXCEÇÃO LOCAL UPLOAD ===\n{ex.Message}");
            return string.Empty;
        }
    }

    public async Task<List<TarefaResumo>> ObterTarefasPorUtenteEDataAsync(string utenteId, DateTime data)
    {
        try
        {
            await ConfigurarTokenAsync();
            string dataFormatada = data.ToString("yyyy-MM-dd");

            // Ajusta esta rota consoante o que tens no teu Backend!
            var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/api/TaskLogs/utente/{utenteId}/dia/{dataFormatada}");

            if (response.IsSuccessStatusCode)
            {
                var rawJson = await response.Content.ReadAsStringAsync();
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dtos = System.Text.Json.JsonSerializer.Deserialize<List<TarefaResumoDto>>(rawJson, options);

                if (dtos == null) return new List<TarefaResumo>();

                return dtos.Select(dto => new TarefaResumo
                {
                    Id = dto.Id,
                    DataHora = dto.DataHora,
                    Titulo = dto.Titulo,
                    Categoria = dto.Categoria,
                    NomeUtente = dto.NomeUtente,
                    EstaConcluida = dto.Concluida,
                    IsAdHoc = dto.IsAdHoc,
                    TimestampExecucao = dto.TimestampExecucao,
                    Notas = dto.Notas,
                    AvatarUtente = dto.AvatarUtente
                }).ToList();
            }
            return new List<TarefaResumo>();
        }
        catch
        {
            return new List<TarefaResumo>();
        }
    }
}