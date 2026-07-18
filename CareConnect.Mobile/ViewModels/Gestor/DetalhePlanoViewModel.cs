using CareConnect.Mobile.Services;
using CareConnect.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Gestor;

// O MAUI vai injetar a rota "?id=..." diretamente na propriedade "PlanoId"
[QueryProperty(nameof(PlanoId), "id")]
public partial class DetalhePlanoViewModel : ObservableObject
{
    private readonly CarePlanService _carePlanService;
    private readonly PatientService _patientService;
    private readonly INotificationService _notificationService;

    [ObservableProperty] private CarePlan? _planoAtual;

    // ⚠️ Recebe o ID da rota automaticamente
    [ObservableProperty] 
    private string _planoId = string.Empty;

    // --- DADOS PARA EDIÇÃO ---
    [ObservableProperty] private string _descricaoEditada = string.Empty;
    [ObservableProperty] private TimeSpan _horaEditada;

    // --- DADOS INFORMATIVOS DO UTENTE E PLANO ---
    [ObservableProperty] private string _nomeUtente = "A carregar...";
    [ObservableProperty] private string _fotoUtente = "avatar_elderly.png";
    [ObservableProperty] private string _tipoPlano = "Cuidado";

    public DetalhePlanoViewModel(CarePlanService carePlanService, PatientService patientService, INotificationService notificationService)
    {
        _carePlanService = carePlanService;
        _patientService = patientService;
        _notificationService = notificationService;
    }

    // Quando o MAUI preenche o "PlanoId", este método dispara sozinho!
    partial void OnPlanoIdChanged(string value)
    {
        Console.WriteLine($"[RASTREIO VM] 1. OnPlanoIdChanged disparou! Valor recebido da rota: '{value}'");
        
        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine("[RASTREIO VM] 1.1. ERRO: O valor recebido está VAZIO!");
            return;
        }

        if (Guid.TryParse(value, out Guid idGuid))
        {
            Console.WriteLine($"[RASTREIO VM] 2. Guid parse com sucesso: {idGuid}");
            _ = CarregarDadosDoPlanoAsync(idGuid);
        }
        else
        {
            Console.WriteLine($"[RASTREIO VM] 2.1. FALHA AO CONVERTER '{value}' PARA GUID!");
        }
    }

    private async Task CarregarDadosDoPlanoAsync(Guid id)
    {
        Console.WriteLine($"[RASTREIO VM] 3. A iniciar CarregarDadosDoPlanoAsync...");
        try
        {
            PlanoAtual = await _carePlanService.GetPlanByIdAsync(id);
            
            if (PlanoAtual != null)
            {
                Console.WriteLine($"[RASTREIO VM] 6. Plano recebido na View! Descrição: {PlanoAtual.Descricao}");
                
                DescricaoEditada = PlanoAtual.Descricao;
                HoraEditada = PlanoAtual.HoraProgramada;
                TipoPlano = PlanoAtual.Tipo.ToString();

                Console.WriteLine($"[RASTREIO VM] 7. A carregar pacientes para procurar a foto...");
                var pacientes = await _patientService.GetMyPatientsAsync();
                var pacienteDono = pacientes.FirstOrDefault(p => p.Id == PlanoAtual.PatientId);
                
                if (pacienteDono != null)
                {
                    Console.WriteLine($"[RASTREIO VM] 8. Paciente dono encontrado: {pacienteDono.Nome}");
                    NomeUtente = pacienteDono.Nome;
                    FotoUtente = string.IsNullOrEmpty(pacienteDono.AvatarUrl) ? "avatar_elderly.png" : pacienteDono.AvatarUrl;
                }
                else
                {
                    Console.WriteLine($"[RASTREIO VM] 8. Paciente dono NÃO encontrado na lista!");
                }
            }
            else
            {
                Console.WriteLine($"[RASTREIO VM] 6. ERRO: O PlanoAtual retornou NULL do serviço!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RASTREIO VM] EXCEÇÃO CRÍTICA NO CARREGAMENTO: {ex.Message}");
            await _notificationService.MostrarErroAsync("Falha ao ler dados: " + ex.Message);
        }
    }
    
    [RelayCommand]
    private async Task VoltarAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task GuardarAlteracoesAsync()
    {
        if (PlanoAtual == null) return;

        try
        {
            PlanoAtual.Descricao = DescricaoEditada;
            PlanoAtual.HoraProgramada = HoraEditada;
            
            await _carePlanService.UpdatePlanAsync(PlanoAtual);
            await _notificationService.MostrarSucessoAsync("Plano atualizado!");
            await Shell.Current.GoToAsync(".."); 
        }
        catch(Exception ex) 
        { 
            await _notificationService.MostrarErroAsync(ex.Message); 
        }
    }

    [RelayCommand]
    private async Task ApagarPlanoAsync()
    {
        if (PlanoAtual == null) return;

        bool confirma = await Shell.Current.DisplayAlert(
            "Atenção", "Tem a certeza que deseja apagar esta tarefa?", "Sim, Apagar", "Cancelar");
        
        if (confirma)
        {
            try
            {
                await _carePlanService.DeletePlanAsync(PlanoAtual.Id);
                await _notificationService.MostrarSucessoAsync("Plano removido com sucesso.");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _notificationService.MostrarErroAsync($"Falha ao apagar: {ex.Message}");
            }
        }
    }
}