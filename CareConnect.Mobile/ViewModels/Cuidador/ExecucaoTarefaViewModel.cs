using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services;

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class ExecucaoTarefaViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly TarefaService _tarefaService;

    [ObservableProperty]
    private TarefaResumo _tarefaSelecionada;

    [ObservableProperty]
    private string _notas = string.Empty;

    [ObservableProperty]
    private string _corBotaoConcluido = "#D1D5DB";

    [ObservableProperty]
    private string _corBotaoNaoRealizado = "#D1D5DB";

    [ObservableProperty]
    private bool _isLoading;

    private bool? _estadoExecucao = null;
    private string? _fotoCaminhoLocal = null;

    public Action? FecharPopupAcao { get; set; }

    public ExecucaoTarefaViewModel(TarefaResumo tarefa, INotificationService notificationService, TarefaService tarefaService)
    {
        TarefaSelecionada = tarefa;
        _notificationService = notificationService;
        _tarefaService = tarefaService;
    }

    [RelayCommand]
    private void MarcarConcluido()
    {
        _estadoExecucao = true;
        CorBotaoConcluido = "#10B981";       // Verde ativo
        CorBotaoNaoRealizado = "#F3F4F6";   // Cinzento inativo
    }

    [RelayCommand]
    private void MarcarNaoRealizado()
    {
        _estadoExecucao = false;
        CorBotaoNaoRealizado = "#EF4444";   // Vermelho ativo
        CorBotaoConcluido = "#F3F4F6";      // Cinzento inativo
    }

    [RelayCommand]
    private async Task TirarFotoAsync()
    {
        try
        {
            string acao = await Application.Current!.Windows[0].Page!.DisplayActionSheet("Anexar Foto", "Cancelar", null, "Tirar Foto", "Escolher da Galeria");

            FileResult? foto = null;
            if (acao == "Tirar Foto" && MediaPicker.Default.IsCaptureSupported)
            {
                foto = await MediaPicker.Default.CapturePhotoAsync();
            }
            else if (acao == "Escolher da Galeria")
            {
                foto = await MediaPicker.Default.PickPhotoAsync();
            }

            if (foto != null)
            {
                _fotoCaminhoLocal = foto.FullPath;
                await _notificationService.MostrarSucessoAsync("Foto selecionada com sucesso!");
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync("Erro ao carregar a foto: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task SalvarRegistoAsync()
    {
        if (_estadoExecucao == null)
        {
            await _notificationService.MostrarToastAsync("Indica se a tarefa foi concluída ou não realizada.");
            return;
        }

        if (IsLoading) return;

        try
        {
            IsLoading = true;

            // 1. Mapeia o booleano para o enum correspondente na API (0 = Pendente, 1 = Realizado / Ajusta conforme o teu CareTaskStatus)
            // Se "Realizado" for o int 1 e "Não Realizado" for outro valor (ex: 2 ou 3):
            int statusInt = _estadoExecucao.Value ? 1 : 2;

            // Se tirou foto, podes fazer o upload primeiro para o S3 se necessário
            if (!string.IsNullOrEmpty(_fotoCaminhoLocal))
            {
                string urlFotoS3 = await _tarefaService.UploadFotoAdHocAsync(_fotoCaminhoLocal);
                if (!string.IsNullOrEmpty(urlFotoS3) && string.IsNullOrEmpty(Notas))
                {
                    Notas = "[Foto anexada]";
                }
            }

            // 2. Chama o serviço para atualizar o status e as notas diretamente na API
            bool sucesso = await _tarefaService.AtualizarEstadoTarefaAsync(TarefaSelecionada.Id, statusInt, Notas);

            if (sucesso)
            {
                TarefaSelecionada.EstaConcluida = _estadoExecucao.Value;
                await _notificationService.MostrarSucessoAsync("Registo guardado com sucesso!");

                // Fecha o popup
                FecharPopupAcao?.Invoke();
            }
            else
            {
                await _notificationService.MostrarErroAsync("Não foi possível atualizar o estado na base de dados.");
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync($"Erro ao gravar: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}