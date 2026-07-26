using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services; // Certifica-te que o namespace do INotificationService está correto

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class ExecucaoTarefaViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private TarefaResumo _tarefaSelecionada;

    [ObservableProperty]
    private string _notas = string.Empty;

    [ObservableProperty]
    private string _corBotaoConcluido = "#D1D5DB";

    [ObservableProperty]
    private string _corBotaoNaoRealizado = "#D1D5DB";

    private bool? _estadoExecucao = null;

    public Action FecharPopupAcao { get; set; }

    // 1. Injetamos o INotificationService aqui
    public ExecucaoTarefaViewModel(TarefaResumo tarefa, INotificationService notificationService)
    {
        TarefaSelecionada = tarefa;
        _notificationService = notificationService;
    }

    [RelayCommand]
    private void MarcarConcluido()
    {
        _estadoExecucao = true;
        CorBotaoConcluido = "#10B981";
        CorBotaoNaoRealizado = "#F3F4F6";
    }

    [RelayCommand]
    private void MarcarNaoRealizado()
    {
        _estadoExecucao = false;
        CorBotaoNaoRealizado = "#EF4444";
        CorBotaoConcluido = "#F3F4F6";
    }

    [RelayCommand]
    private async Task TirarFotoAsync()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            try
            {
                FileResult foto = await MediaPicker.Default.CapturePhotoAsync();

                if (foto != null)
                {
                    string fotoCaminho = foto.FullPath;
                    // 2. Usamos o serviço para notificações simples
                    _notificationService.MostrarSucessoAsync("Foto anexada com sucesso!");
                }
            }
            catch (Exception ex)
            {
                _notificationService.MostrarErroAsync("Erro: Não foi possível abrir a câmara.");
            }
        }
        else
        {
            _notificationService.MostrarAvisoAsync("O teu dispositivo não suporta captura de foto.");
        }
    }

    [RelayCommand]
    private async Task SalvarRegistoAsync()
    {
        if (_estadoExecucao == null)
        {
            _notificationService.MostrarToastAsync("Indica se a tarefa foi concluída ou não realizada.");
            return;
        }

        // EXEMPLO DE PERGUNTA (Mantendo o DisplayAlert nativo para decisões)
        // bool confirmar = await Application.Current.MainPage.DisplayAlert("Confirmar", "Deseja mesmo guardar este registo?", "Sim", "Não");
        // if (!confirmar) return;

        await Task.Delay(300);

        TarefaSelecionada.EstaConcluida = _estadoExecucao.Value;
        FecharPopupAcao?.Invoke();
    }
}