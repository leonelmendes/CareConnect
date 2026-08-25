using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace CareConnect.Mobile.ViewModels.Gestor;

// Recebe os parâmetros da navegação
[QueryProperty(nameof(UtenteId), "UtenteId")]
[QueryProperty(nameof(NomeUtente), "NomeUtente")]
public partial class RelatorioDiarioViewModel : ObservableObject
{
    private readonly TarefaService _tarefaService;

    [ObservableProperty]
    private string _utenteId;

    [ObservableProperty]
    private string _nomeUtente;

    [ObservableProperty]
    private string _nomeCuidadorResponsavel = "Vários/Não definido"; // Opcional: ajustar consoante a lógica

    [ObservableProperty]
    private DateTime _dataRelatorio = DateTime.Today;

    [ObservableProperty]
    private int _totalConcluidas;

    [ObservableProperty]
    private int _totalPendentes;

    public ObservableCollection<TarefaResumo> TarefasDoRelatorio { get; set; } = new();

    public RelatorioDiarioViewModel(TarefaService tarefaService)
    {
        _tarefaService = tarefaService;
        QuestPDF.Settings.License = LicenseType.Community; // Necessário para o QuestPDF
    }

    // Ocorre sempre que o UtenteId é preenchido pela navegação
    partial void OnUtenteIdChanged(string value)
    {
        CarregarDados();
    }

    partial void OnDataRelatorioChanged(DateTime value)
    {
        CarregarDados();
    }

    private async void CarregarDados()
    {
        if (string.IsNullOrEmpty(UtenteId)) return;

        var tarefas = await _tarefaService.ObterTarefasPorUtenteEDataAsync(UtenteId, DataRelatorio);

        TarefasDoRelatorio.Clear();
        foreach (var t in tarefas) TarefasDoRelatorio.Add(t);

        TotalConcluidas = TarefasDoRelatorio.Count(t => t.EstaConcluida);
        TotalPendentes = TarefasDoRelatorio.Count(t => !t.EstaConcluida && !t.IsAdHoc);
    }

    [RelayCommand]
    private async Task PartilharRelatorioPdfAsync()
    {
        try
        {
            // 1. Prepara o URL da tua API
            string dataFormatada = DataRelatorio.ToString("yyyy-MM-dd");
            string urlDaApi = $"{Constants.BaseUrl}/api/Relatorios/gerar-pdf/{UtenteId}/dia/{dataFormatada}";

            using var httpClient = new HttpClient();

            // Se a API precisar de Token, junta-o aqui:
            // var token = await SecureStorage.Default.GetAsync("auth_token");
            // httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 2. Pede o ficheiro à API
            var response = await httpClient.GetAsync(urlDaApi);

            if (response.IsSuccessStatusCode)
            {
                // 3. Guarda o ficheiro no telemóvel
                var filePath = Path.Combine(FileSystem.CacheDirectory, $"Relatorio_{NomeUtente?.Replace(" ", "")}_{DataRelatorio:ddMMyyyy}.pdf");

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.OpenWrite(filePath);
                await stream.CopyToAsync(fileStream);
                fileStream.Close(); // Garante que o ficheiro está fechado antes de tentar partilhar

                // 4. Partilha usando a janela nativa do Android/iOS
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Partilhar Relatório",
                    File = new ShareFile(filePath)
                });
            }
            else
            {
                // Mostra um erro se a API falhar
                await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível gerar o PDF no servidor.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao gerar PDF: {ex.Message}");
        }
    }
}