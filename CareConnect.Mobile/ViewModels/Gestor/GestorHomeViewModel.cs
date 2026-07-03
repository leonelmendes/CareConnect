using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CareConnect.Mobile.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CareConnect.Mobile.ViewModels.Gestor;

public partial class GestorHomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nomeGestor = "Leonel Francisco";

    public ObservableCollection<PacienteResumo> Pacientes { get; } = new();
    public ObservableCollection<AlertaRecente> Alertas { get; } = new();

    // --- DADOS PARA O GRÁFICO DONUT (Daily Summary) ---
    public ISeries[] ResumoDiarioSeries { get; set; } =
    {
        new PieSeries<int> { Values = new[] { 32 }, Name = "Completed", InnerRadius = 50, Fill = new SolidColorPaint(SKColor.Parse("#10B981")) },
        new PieSeries<int> { Values = new[] { 7 }, Name = "In Progress", InnerRadius = 50, Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")) },
        new PieSeries<int> { Values = new[] { 8 }, Name = "Pending", InnerRadius = 50, Fill = new SolidColorPaint(SKColor.Parse("#E5E7EB")) }
    };

    // --- DADOS PARA O GRÁFICO DE BARRAS (Care Tasks Activity) ---
    public ISeries[] AtividadeSeries { get; set; } =
    {
        new ColumnSeries<int>
        {
            Values = new[] { 2, 4, 3, 7, 4, 5, 3 },
            Fill = new SolidColorPaint(SKColor.Parse("#2563EB")),
            Rx = 6, // Arredonda os cantos superiores das barras (como no Figma!)
            Ry = 6
        }
    };

    // Esconde as linhas de fundo do eixo Y para ficar limpo
    public Axis[] EixoY { get; set; } = { new Axis { IsVisible = false } };
    
    // Rótulos do eixo X (00h, 08h, etc)
    public Axis[] EixoX { get; set; } = { 
        new Axis { 
            Labels = new[] { "00h", "", "08h", "", "16h", "", "24h" },
            LabelsPaint = new SolidColorPaint(SKColor.Parse("#757575")),
            TextSize = 12
        } 
    };

    public GestorHomeViewModel()
    {
        CarregarDadosSimulados();
    }

    private void CarregarDadosSimulados()
    {
        Pacientes.Add(new PacienteResumo { Nome = "John Davis", Imagem = "avatar_1", EstaOnline = false });
        Pacientes.Add(new PacienteResumo { Nome = "Maria Silva", Imagem = "avatar_2", EstaOnline = true });
        Pacientes.Add(new PacienteResumo { Nome = "João", Imagem = "avatar_3", EstaOnline = true });
        Pacientes.Add(new PacienteResumo { Nome = "Ana Costa", Imagem = "avatar_4", EstaOnline = false });

        Alertas.Add(new AlertaRecente { Titulo = "João Silva", Descricao = "Sinais vitais fora do intervalo", Tempo = "Há 15 min", CorIcone = "#FEE2E2", ImagemIcone = "icon_alert_red" });
        Alertas.Add(new AlertaRecente { Titulo = "Ana Ferreira", Descricao = "Lembrete de medicamento esquecido", Tempo = "Há 45 min", CorIcone = "#FEF3C7", ImagemIcone = "icon_alert_yellow" });
        Alertas.Add(new AlertaRecente { Titulo = "Carlos Mendes", Descricao = "Avaliação de dor muito elevada", Tempo = "Há 1 h", CorIcone = "#FEE2E2", ImagemIcone = "icon_alert_red" });
    }
}