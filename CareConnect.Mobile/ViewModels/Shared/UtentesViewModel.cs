using System.Collections.ObjectModel;
using CareConnect.Shared.Models;
using CareConnect.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CareConnect.Mobile.Messages;

namespace CareConnect.Mobile.ViewModels.Shared;

public partial class UtentesViewModel : ObservableObject
{
    private readonly PatientService _patientService;
    private readonly INotificationService _notificationService;

    // Cache local com todos os pacientes vindos da API para não perdermos os dados ao filtrar
    private List<Patient> _todosUtentesCache = new();
    private string _filtroDoencaAtual = "Todas";

    [ObservableProperty]
    private ObservableCollection<Patient> _listaUtentes = new();
    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isRefreshing;

    // Esta propriedade está ligada à Entry. O MVVM Toolkit chama o método abaixo automaticamente quando muda.
    [ObservableProperty]
    private string _textoPesquisa;
    [ObservableProperty]
    private bool _isFiltroAberto;

    [ObservableProperty]
    private List<string> _listaDoencasFiltro = new();

    public bool IsGestor => Preferences.Default.Get("auth_profile", "Gestor") == "Gestor";

    public UtentesViewModel(PatientService patientService, INotificationService notificationService)
    {
        _patientService = patientService;
        _notificationService = notificationService;

        WeakReferenceMessenger.Default.Register<PatientUpdatedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Invoca silenciosamente o comando que tu já tens para atualizar a API
                AtualizarListaCommand.Execute(null); 
            });
        });
    }

    // Método mágico do Toolkit: dispara sempre que o utilizador digita ou apaga uma letra
    partial void OnTextoPesquisaChanged(string value)
    {
        AplicarFiltros();
    }

    [RelayCommand]
    public async Task AtualizarListaAsync()
    {
        IsRefreshing = true;
        await CarregarUtentesAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task CarregarUtentesAsync()
    {
        try
        {
            var pacientesDb = await _patientService.GetMyPatientsAsync();
            
            // Guardamos na cache
            _todosUtentesCache = pacientesDb;
            
            // Aplicamos os filtros (caso haja alguma pesquisa ou filtro ativo ao recarregar)
            AplicarFiltros();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar utentes: {ex.Message}");
        }
    }

    // A lógica central que cruza a pesquisa por texto com o filtro de doenças
    private void AplicarFiltros()
    {
        var filtrados = _todosUtentesCache.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(TextoPesquisa))
        {
            filtrados = filtrados.Where(p => 
                !string.IsNullOrEmpty(p.Nome) && 
                p.Nome.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase));
        }

        if (_filtroDoencaAtual != "Todas")
        {
            filtrados = filtrados.Where(p => 
                !string.IsNullOrEmpty(p.CondicoesMedicas) && 
                p.CondicoesMedicas.Contains(_filtroDoencaAtual, StringComparison.OrdinalIgnoreCase));
        }

        ListaUtentes.Clear();
        foreach (var paciente in filtrados)
        {
            ListaUtentes.Add(paciente);
        }

        // Se a lista estiver vazia, o IsEmpty fica true e mostra a mensagem
        IsEmpty = ListaUtentes.Count == 0;
    }

    [RelayCommand]
    private async Task MostrarFiltrosAsync()
    {
        // Puxa as doenças únicas de todos os utentes (ignorando nulos e vazios)
        var doencas = _todosUtentesCache
            .Where(p => !string.IsNullOrWhiteSpace(p.CondicoesMedicas))
            .Select(p => p.CondicoesMedicas)
            .Distinct()
            .ToList();

        if (!doencas.Any())
        {
            await Shell.Current.DisplayAlert("Sem filtros", "Não existem doenças registadas nos utentes.", "OK");
            return;
        }

        // Adiciona a opção para limpar o filtro no topo
        doencas.Insert(0, "Todas");

        // Mostra o menu nativo na parte inferior do ecrã
        var acao = await Shell.Current.DisplayActionSheet("Filtrar por Doença", "Cancelar", null, doencas.ToArray());

        if (!string.IsNullOrEmpty(acao) && acao != "Cancelar")
        {
            _filtroDoencaAtual = acao;
            AplicarFiltros();
        }
    }

    [RelayCommand]
    private async Task VerDetalhesAsync(Patient pacienteSelecionado)
    {
        if (pacienteSelecionado == null) return;

        var parametros = new Dictionary<string, object>
        {
            { "UtenteSelecionado", pacienteSelecionado }
        };

        await Shell.Current.GoToAsync("DetalheUtenteView", parametros);
    }

    [RelayCommand]
    private async Task AlternarFiltrosAsync()
    {
        if (IsFiltroAberto)
        {
            IsFiltroAberto = false;
            return;
        }

        // Gera a lista de doenças dinamicamente
        var doencas = _todosUtentesCache
            .Where(p => !string.IsNullOrWhiteSpace(p.CondicoesMedicas))
            .Select(p => p.CondicoesMedicas)
            .Distinct()
            .ToList();

        if (!doencas.Any())
        {
            //Shell.Current.DisplayAlert("Aviso", "Ainda não existem utentes com doenças registadas.", "OK");
            await _notificationService.MostrarAvisoAsync("Ainda não existem utentes com doenças registadas.");
            return;
        }

        doencas.Insert(0, "Todas");
        ListaDoencasFiltro = doencas;
        
        IsFiltroAberto = true;
    }

    [RelayCommand]
    private void SelecionarDoenca(string doencaSelecionada)
    {
        _filtroDoencaAtual = doencaSelecionada;
        IsFiltroAberto = false; // Fecha o menu
        AplicarFiltros();       // Filtra a lista
    }

    // COMANDO PARA FECHAR SE CLICAR FORA DO MENU
    [RelayCommand]
    private void FecharFiltro()
    {
        IsFiltroAberto = false;
    }

    [RelayCommand]
    private async Task AdicionarUtenteAsync()
    {
        if (!IsGestor) return;
        await Shell.Current.GoToAsync("AdicionarUtenteView");
    }
}