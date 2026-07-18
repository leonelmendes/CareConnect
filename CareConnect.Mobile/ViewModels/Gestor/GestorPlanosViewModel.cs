using CareConnect.Mobile.Services;
using CareConnect.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CareConnect.Mobile.ViewModels.Gestor;

public partial class GestorPlanosViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly PatientService _patientService;
    private readonly CarePlanService _carePlanService;

    // ⚠️ Propriedade que controla a animação de rotação do "arrastar para baixo"
    [ObservableProperty] 
    private bool _isRefreshing;

    [ObservableProperty]
    private string _filtroAtual = "Todos";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;
    public bool IsNotLoading => !IsLoading;

    [ObservableProperty]
    private ObservableCollection<PlanoCuidadoUI> _planos = new();
    private List<PlanoCuidadoUI> _todosOsPlanosEmCache = new();

    public GestorPlanosViewModel(
        INotificationService notificationService, 
        PatientService patientService, 
        CarePlanService carePlanService)
    {
        _notificationService = notificationService;
        _patientService = patientService;
        _carePlanService = carePlanService;
    }

    [RelayCommand]
    public async Task CarregarPlanosAsync()
    {
        // ⚠️ REMOVIDO o "if (IsRefreshing) return;" para não abortar o comando
        if (IsLoading) return;
        
        try
        {
            IsLoading = true;
            _todosOsPlanosEmCache.Clear(); 

            // Criamos uma lista temporária para não "assustar" o XAML
            var novaLista = new ObservableCollection<PlanoCuidadoUI>();

            var pacientes = await _patientService.GetMyPatientsAsync();

            foreach (var paciente in pacientes)
            {
                var planosDoPaciente = await _carePlanService.GetPlansByPatientIdAsync(paciente.Id);
                
                foreach (var plano in planosDoPaciente)
                {
                    var planoUI = new PlanoCuidadoUI
                    {
                        Id = plano.Id,
                        Titulo = plano.Tipo.ToString(),
                        Descricao = plano.Descricao,
                        Horarios = plano.HoraProgramada.ToString(@"hh\:mm"),
                        UtenteNome = paciente.Nome,
                        UtenteFoto = string.IsNullOrEmpty(paciente.AvatarUrl) ? "avatar_elderly.png" : paciente.AvatarUrl,
                        Icone = ObterIcone(plano.Tipo.ToString()),
                        CorFundoIcone = ObterCorFundo(plano.Tipo.ToString())
                    };
                    
                    novaLista.Add(planoUI);
                    _todosOsPlanosEmCache.Add(planoUI);
                }
            }

            // ⚠️ SOLUÇÃO DO BUG DO FILTRO: Substitui a lista de uma vez só!
            Planos = novaLista;
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync($"Erro: {ex.Message}");
        }
        finally 
        { 
            IsLoading = false; 
            IsRefreshing = false; // ⚠️ Desliga o spinner à força!
        }
    }

    [RelayCommand]
    private void FiltrarPlanos(string tipoFiltro)
    {
        FiltroAtual = tipoFiltro; // Atualiza a variável que controla a cor do botão

        if (tipoFiltro == "Todos")
        {
            Planos = new ObservableCollection<PlanoCuidadoUI>(_todosOsPlanosEmCache);
            return;
        }

        // Ignora maiúsculas/minúsculas para evitar bugs se o Enum estiver diferente
        var planosFiltrados = _todosOsPlanosEmCache
            .Where(p => p.Titulo.Equals(tipoFiltro, StringComparison.OrdinalIgnoreCase))
            .ToList();
            
        Planos = new ObservableCollection<PlanoCuidadoUI>(planosFiltrados);
    }

    [RelayCommand]
    private async Task AbrirCriarPlanoAsync() => await Shell.Current.GoToAsync("CriarPlanoCuidadoView");

    [RelayCommand]
    private async Task VerDetalhesPlanoAsync(PlanoCuidadoUI planoSelecionado)
    {
        Console.WriteLine($"[CLIQUE] ID selecionado: {planoSelecionado?.Id}");
    
        if (planoSelecionado == null || planoSelecionado.Id == Guid.Empty) 
            return;
        
        // Passamos o ID pela rota como texto, é muito mais seguro no MAUI
        await Shell.Current.GoToAsync($"DetalhePlanoView?id={planoSelecionado.Id}");
    }

    private string ObterIcone(string tipo) => tipo switch
    {
        "Medicacao" => "💊",
        "Higiene" => "🚿",
        "Terapia" => "🩺",
        "Alimentacao" => "🍲",
        _ => "📝"
    };

    private Color ObterCorFundo(string tipo) => tipo switch
    {
        "Medicacao" => Color.FromArgb("#EFF6FF"),
        "Higiene" => Color.FromArgb("#F0FDF4"),
        "Terapia" => Color.FromArgb("#FAF5FF"),
        "Alimentacao" => Color.FromArgb("#FFF7ED"),
        _ => Color.FromArgb("#F3F4F6")
    };
}