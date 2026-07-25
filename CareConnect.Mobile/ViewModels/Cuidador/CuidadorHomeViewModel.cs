using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CareConnect.Mobile.ViewModels;

public partial class CuidadorHomeViewModel : ObservableObject
{
    private readonly TarefaService _tarefaService;

    [ObservableProperty]
    private string _nomeCuidador = "Cuidador(a)";

    [ObservableProperty]
    private string _dataAtual;

    [ObservableProperty]
    private int _tarefasConcluidas;

    [ObservableProperty]
    private int _tarefasPendentes;

    [ObservableProperty]
    private string _progressoPercentual = "0%";

    // Propriedade para o LiveCharts (Gráfico Circular)
    [ObservableProperty]
    private ISeries[] _resumoDiarioSeries;

    public ObservableCollection<TarefaResumo> ProximasTarefas { get; set; } = new();

    public CuidadorHomeViewModel(TarefaService tarefaService)
    {
        _tarefaService = tarefaService;
        DataAtual = DateTime.Now.ToString("dd 'de' MMMM, yyyy", new System.Globalization.CultureInfo("pt-PT"));
        
        // Inicializa o gráfico vazio para não dar erro ao abrir a página
        AtualizarGrafico(0, 1); 
    }

    [RelayCommand]
    private async Task CarregarDadosIniciaisAsync()
    {
        var nomeGuardado = Preferences.Default.Get("user_nome", string.Empty);
        NomeCuidador = string.IsNullOrWhiteSpace(nomeGuardado) ? "Cuidador(a)" : nomeGuardado;

        // Busca as tarefas à API através do serviço
        var tarefasDaApi = await _tarefaService.ObterTarefasHojeAsync();

        ProximasTarefas.Clear();
        TarefasConcluidas = 0;
        TarefasPendentes = 0;

        foreach (var tarefa in tarefasDaApi)
        {
            ProximasTarefas.Add(tarefa);
            if (tarefa.EstaConcluida)
                TarefasConcluidas++;
            else
                TarefasPendentes++;
        }

        var total = TarefasConcluidas + TarefasPendentes;
        
        if (total > 0)
        {
            ProgressoPercentual = $"{(int)((double)TarefasConcluidas / total * 100)}%";
        }

        // Atualiza a UI do LiveCharts
        AtualizarGrafico(TarefasConcluidas, TarefasPendentes);
    }

    private void AtualizarGrafico(int concluidas, int pendentes)
    {
        // Se não houver tarefas, mostramos um gráfico cinzento (pendente)
        if (concluidas == 0 && pendentes == 0) pendentes = 1;

        ResumoDiarioSeries = new ISeries[]
        {
            new PieSeries<int>
            {
                Values = new[] { concluidas },
                Name = "Concluídas",
                Fill = new SolidColorPaint(SKColor.Parse("#10B981")), // Verde
                InnerRadius = 50,
                MaxRadialColumnWidth = 15
            },
            new PieSeries<int>
            {
                Values = new[] { pendentes },
                Name = "Pendentes",
                Fill = new SolidColorPaint(SKColor.Parse("#F59E0B")), // Laranja/Amarelo
                InnerRadius = 50,
                MaxRadialColumnWidth = 15
            }
        };
    }

    [RelayCommand]
    private async Task AbrirRegistoAdHoc()
    {
        // Navega para a página de criação de tarefa Ad-Hoc
        // Certifica-te de que a rota "RegistoAdHocView" está registada no teu AppShell
        await Shell.Current.GoToAsync("RegistoAdHocView");
    }

    [RelayCommand]
    private async Task AbrirNotas()
    {
        // Placeholder para outra ação rápida
        await Application.Current!.MainPage!.DisplayAlertAsync("Notas", "Funcionalidade de notas rápidas em breve.", "OK");
    }

}