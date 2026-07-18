using CareConnect.Mobile.Services;
using CareConnect.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CareConnect.Mobile.ViewModels.Gestor;

public partial class CriarPlanoCuidadoViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly PatientService _patientService;
    private readonly CarePlanService _carePlanService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;
    public bool IsNotLoading => !IsLoading;

    // --- SELEÇÃO DE UTENTE ---
    public ObservableCollection<Patient> PacientesAtivos { get; } = new();
    
    [ObservableProperty] 
    private Patient? _pacienteSelecionado;

    // --- ESTADOS DOS BOTÕES DE TIPO DE CUIDADO ---
    [ObservableProperty] private bool _isMedicacao = true;
    [ObservableProperty] private bool _isHigiene;
    [ObservableProperty] private bool _isTerapia;
    [ObservableProperty] private bool _isAlimentacao;
    private PlanType _tipoSelecionado = PlanType.Medicacao; // Usa o teu Enum real aqui

    // --- ESTADOS DOS BOTÕES DE FREQUÊNCIA ---
    [ObservableProperty] private bool _isDiario = true;
    [ObservableProperty] private bool _isSemanal;
    private string _frequenciaSelecionada = "Diário";

    // --- DADOS DO FORMULÁRIO ---
    [ObservableProperty] private string _descricaoTarefa = string.Empty;
    [ObservableProperty] private TimeSpan _horarioSelecionado = new TimeSpan(8, 0, 0);

    public CriarPlanoCuidadoViewModel(INotificationService notificationService, PatientService patientService, CarePlanService carePlanService)
    {
        _notificationService = notificationService;
        _patientService = patientService;
        _carePlanService = carePlanService;
        _ = CarregarPacientesAsync();
    }

    private async Task CarregarPacientesAsync()
    {
        try
        {
            var pacientes = await _patientService.GetMyPatientsAsync();
            PacientesAtivos.Clear();
            foreach (var p in pacientes)
            {
                if (p.Ativo) PacientesAtivos.Add(p);
            }
            
            // Auto-seleciona o primeiro paciente se existir
            if (PacientesAtivos.Any()) PacienteSelecionado = PacientesAtivos.First();
        }
        catch (Exception ex) { await _notificationService.MostrarErroAsync("Erro ao carregar utentes."); }
    }

    // --- COMANDOS PARA MUDAR A SELEÇÃO VISUAL ---
    [RelayCommand]
    private void SelecionarTipo(string tipo)
    {
        IsMedicacao = tipo == "Medicacao";
        IsHigiene = tipo == "Higiene";
        IsTerapia = tipo == "Terapia";
        IsAlimentacao = tipo == "Alimentacao";

        // Mapeia para o Enum da API
        if (IsMedicacao) _tipoSelecionado = PlanType.Medicacao;
        else if (IsHigiene) _tipoSelecionado = PlanType.Higiene;
        else if (IsTerapia) _tipoSelecionado = PlanType.Terapia; // Ajusta conforme o teu enum
        else if (IsAlimentacao) _tipoSelecionado = PlanType.Alimentacao; // Ajusta conforme o teu enum
    }

    [RelayCommand]
    private void SelecionarFrequencia(string frequencia)
    {
        IsDiario = frequencia == "Diário";
        IsSemanal = frequencia == "Semanal";
        _frequenciaSelecionada = frequencia;
    }

    [RelayCommand]
    private async Task VoltarAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task GuardarPlanoAsync()
    {
        if (PacienteSelecionado == null)
        {
            await _notificationService.MostrarAvisoAsync("Por favor, selecione um utente primeiro.");
            return;
        }

        if (string.IsNullOrWhiteSpace(DescricaoTarefa))
        {
            await _notificationService.MostrarAvisoAsync("Preencha a descrição da tarefa.");
            return;
        }

        if (IsLoading) return;

        try
        {
            IsLoading = true;

            var novoPlano = new CarePlan
            {
                PatientId = PacienteSelecionado.Id,
                Tipo = _tipoSelecionado,
                Descricao = DescricaoTarefa,
                HoraProgramada = HorarioSelecionado,
                Frequencia = _frequenciaSelecionada
            };

            await _carePlanService.CreatePlanAsync(novoPlano);

            await _notificationService.MostrarSucessoAsync("Plano guardado com sucesso!");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync($"Falha ao gravar: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}