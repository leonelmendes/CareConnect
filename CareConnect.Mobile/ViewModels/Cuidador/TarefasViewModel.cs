using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services;
using CareConnect.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class TarefasViewModel : ObservableObject
{
    private readonly TarefaService _tarefaService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string _nomeCuidador = "Cuidador(a)";

    [ObservableProperty]
    private ObservableCollection<TarefaResumo> _tarefas = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filtroSelecionado = "Últimos 7 dias";

    [ObservableProperty]
    private DateTime _dataSelecionada = DateTime.Today;

    public TarefasViewModel(TarefaService tarefaService, INotificationService notificationService)
    {
        _tarefaService = tarefaService;
        _notificationService = notificationService;

        _ = CarregarDadosIniciaisAsync();
    }

    private async Task CarregarDadosIniciaisAsync()
    {
        // Puxa o nome do utilizador guardado nas preferências durante o login
        NomeCuidador = Preferences.Default.Get("user_name", "Cuidador(a)");
        await CarregarTarefasAsync();
    }

    [RelayCommand]
    public async Task CarregarTarefasAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;

            // Para abranger o histórico (como a imagem indica "Histórico de Tarefas"), 
            // podemos requisitar com base na data de hoje ou alargar o período.
            var lista = await _tarefaService.ObterTarefasPorDataAsync(DateTime.Today);

            Tarefas.Clear();
            foreach (var tarefa in lista)
            {
                Tarefas.Add(tarefa);
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync("Erro ao carregar tarefas: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnFiltroSelecionadoChanged(string value)
    {
        // Aqui podes ajustar a query consoante o filtro escolhido (Hoje, 7 dias, etc.)
        _ = CarregarTarefasAsync();
    }

    partial void OnDataSelecionadaChanged(DateTime value)
    {
        _ = CarregarTarefasPorDataAsync(value);
    }

    private async Task CarregarTarefasPorDataAsync(DateTime dataFiltro)
    {
        Tarefas.Clear();

        // Chama o teu método do serviço passando a data do calendário
        var tarefasDaApi = await _tarefaService.ObterTarefasPorDataAsync(dataFiltro);

        foreach (var tarefa in tarefasDaApi)
        {
            Tarefas.Add(tarefa);
        }
    }

}