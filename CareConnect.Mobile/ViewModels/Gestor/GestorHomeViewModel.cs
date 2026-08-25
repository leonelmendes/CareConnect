using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CareConnect.Mobile.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using CareConnect.Mobile.Services;
using CareConnect.Shared.Models;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Gestor;

public partial class GestorHomeViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly PatientService _patientService;
    private readonly TarefaService _tarefaService;

    [ObservableProperty]
    private string _nomeGestor = "Carregando...";

    [ObservableProperty]
    private int _numeroAlertas;

    [ObservableProperty]
    private string _totalUtentesAtivos = "0";

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<Patient> Pacientes { get; } = new();

    public ObservableCollection<AlertaRecente> Alertas { get; } = new();

    public ISeries[] ResumoDiarioSeries { get; set; } =
    {
        new PieSeries<int> { Values = new[] { 32 }, Name = "Completed", InnerRadius = 50, Fill = new SolidColorPaint(SKColor.Parse("#10B981")) },
        new PieSeries<int> { Values = new[] { 7 }, Name = "In Progress", InnerRadius = 50, Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")) },
        new PieSeries<int> { Values = new[] { 8 }, Name = "Pending", InnerRadius = 50, Fill = new SolidColorPaint(SKColor.Parse("#E5E7EB")) }
    };

    public ISeries[] AtividadeSeries { get; set; } =
    {
        new ColumnSeries<int>
        {
            Values = new[] { 2, 4, 3, 7, 4, 5, 3 },
            Fill = new SolidColorPaint(SKColor.Parse("#2563EB")),
            Rx = 6, 
            Ry = 6
        }
    };

    public Axis[] EixoY { get; set; } = { new Axis { IsVisible = false } };
    
    public Axis[] EixoX { get; set; } = { 
        new Axis { 
            Labels = new[] { "00h", "", "08h", "", "16h", "", "24h" },
            LabelsPaint = new SolidColorPaint(SKColor.Parse("#757575")),
            TextSize = 12
        } 
    };

    public GestorHomeViewModel(INotificationService notificationService, PatientService patientService, TarefaService tarefaService)
    {
        _notificationService = notificationService;
        _patientService = patientService;
        _tarefaService = tarefaService;
    }

    [RelayCommand]
    private async Task CarregarDadosHomeAsync()
    {
        if (Pacientes.Count > 0)
        {
            IsLoading = true;
        }

        try
        {
            IsLoading = true;

            var nomeGuardado = Preferences.Default.Get("user_nome", "Gestor");
            NomeGestor = string.IsNullOrWhiteSpace(nomeGuardado) ? "Gestor" : nomeGuardado;

            var listaPacientes = await _patientService.GetMyPatientsAsync();

            Pacientes.Clear();
            int ativosCount = 0;

            foreach (var p in listaPacientes)
            {
                Pacientes.Add(p);
                if (p.Ativo) ativosCount++;
            }

            TotalUtentesAtivos = ativosCount.ToString();

            var tarefasDeHoje = await _tarefaService.ObterTarefasPorDataAsync(DateTime.Today);
            CalcularAlertas(tarefasDeHoje);
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync($"Erro ao carregar Home: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }

        Alertas.Clear();
        Alertas.Add(new AlertaRecente { Titulo = "João Silva", Descricao = "Sinais vitais fora do intervalo", Tempo = "Há 15 min", CorIcone = "#FEE2E2", ImagemIcone = "icon_alert_red" });
        Alertas.Add(new AlertaRecente { Titulo = "Ana Ferreira", Descricao = "Lembrete de medicamento esquecido", Tempo = "Há 45 min", CorIcone = "#FEF3C7", ImagemIcone = "icon_alert_yellow" });
        Alertas.Add(new AlertaRecente { Titulo = "Carlos Mendes", Descricao = "Avaliação de dor muito elevada", Tempo = "Há 1 h", CorIcone = "#FEE2E2", ImagemIcone = "icon_alert_red" });
    }

    [RelayCommand]
    private async Task AbrirNovoUtenteAsync()
    {
        await Shell.Current.GoToAsync("AdicionarUtenteView");
    }

    [RelayCommand]
    private async Task AbrirCriarPlanoAsync()
    {
        await Shell.Current.GoToAsync("CriarPlanoCuidadoView");
    }

    [RelayCommand]
    private async Task AbrirTodosUtentesAsync()
    {
        await Shell.Current.GoToAsync("//UtentesView");
    }

    [RelayCommand]
    private async Task AbrirRelatoriosAsync()
    {
        await Shell.Current.GoToAsync("SelecaoUtenteRelatorioView");
    }

    private void CalcularAlertas(List<TarefaResumo> todasTarefasDoDia)
    {
        NumeroAlertas = todasTarefasDoDia.Count(t => t.IsAdHoc);
    }
}