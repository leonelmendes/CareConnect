using CareConnect.Mobile.Services;
using CareConnect.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
// Ajusta os namespaces consoante a tua estrutura
// using CareConnect.Mobile.Models; 
// using CareConnect.Mobile.Services;

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class RegistoAdHocViewModel : ObservableObject
{
    private readonly TarefaService _tarefaService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private ObservableCollection<UtenteResumo> _utentesDisponiveis;

    [ObservableProperty]
    private UtenteResumo _utenteSelecionado;

    [ObservableProperty]
    private string _tituloTarefa;

    [ObservableProperty]
    private string _notas;

    [ObservableProperty]
    private bool _isBusy;

    public RegistoAdHocViewModel(TarefaService tarefaService, INotificationService notificationService)
    {
        _tarefaService = tarefaService;
        _notificationService = notificationService;
        UtentesDisponiveis = new ObservableCollection<UtenteResumo>();
        
        CarregarUtentesDeTeste();
    }

    private void CarregarUtentesDeTeste()
    {
        UtentesDisponiveis.Add(new UtenteResumo { Id = Guid.NewGuid(), Nome = "Maria Silva" });
        UtentesDisponiveis.Add(new UtenteResumo { Id = Guid.NewGuid(), Nome = "João Santos" });
    }

    [RelayCommand]
    private async Task GuardarAdHocAsync()
    {
        if (UtenteSelecionado == null)
        {
            await _notificationService.MostrarAvisoAsync("Por favor, selecione um utente.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TituloTarefa))
        {
            await _notificationService.MostrarAvisoAsync("Por favor, indique o que foi feito.");
            return;
        }

        IsBusy = true;

        try
        {
            // 1. Preparar o DTO com os dados do formulário
            var novoRegisto = new RegistoAdHocDto
            {
                UtenteId = UtenteSelecionado.Id,
                Titulo = TituloTarefa,
                Notas = Notas ?? string.Empty,
                DataHora = DateTime.UtcNow // Ou DateTime.Now, dependendo de como a tua API lida com datas
            };

            // 2. Enviar para a API
            var sucesso = await _tarefaService.RegistarAdHocAsync(novoRegisto);

            if (sucesso)
            {
                await _notificationService.MostrarSucessoAsync("Registo Ad-Hoc guardado com sucesso!");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await _notificationService.MostrarErroAsync("Não foi possível guardar o registo na base de dados.");
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync($"Ocorreu um erro: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
