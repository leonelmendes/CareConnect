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

    private List<Patient> _todosUtentesCache = new();
    private string _filtroDoencaAtual = "Todas";

    [ObservableProperty]
    private ObservableCollection<Patient> _listaUtentes = new();

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _textoPesquisa;

    [ObservableProperty]
    private bool _isFiltroAberto;

    [ObservableProperty]
    private List<string> _listaDoencasFiltro = new();

    // A nossa bússola: diz-nos quem está a usar a App
    public bool IsGestor => Preferences.Default.Get("auth_profile", "Gestor") == "Gestor";

    public UtentesViewModel(PatientService patientService, INotificationService notificationService)
    {
        _patientService = patientService;
        _notificationService = notificationService;

        WeakReferenceMessenger.Default.Register<PatientUpdatedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AtualizarListaCommand.Execute(null);
            });
        });
    }

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

    // 🔥 AQUI ESTÁ A ÚNICA GRANDE MUDANÇA 🔥
    [RelayCommand]
    private async Task CarregarUtentesAsync()
    {
        try
        {
            List<Patient> pacientesDb;

            // O nosso "Polícia Sinaleiro"
            if (IsGestor)
            {
                // Se for o gestor, vai buscar todos os utentes da instituição
                // Nota: Garante que tens este método no teu PatientService!
                pacientesDb = await _patientService.GetMyPatientsAsync();
            }
            else
            {
                // Se for cuidador, vai buscar apenas os utentes que lhe foram atribuídos
                // (O endpoint novo que fizemos na API)
                pacientesDb = await _patientService.GetMeusPacientesAsync();
            }

            // O resto continua igual: guardamos na cache e aplicamos a pesquisa
            _todosUtentesCache = pacientesDb;
            AplicarFiltros();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar utentes: {ex.Message}");
            // Podes adicionar aqui um _notificationService.MostrarErroAsync se quiseres
        }
    }

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

        IsEmpty = ListaUtentes.Count == 0;
    }

    [RelayCommand]
    private async Task MostrarFiltrosAsync()
    {
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

        doencas.Insert(0, "Todas");
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

        var doencas = _todosUtentesCache
            .Where(p => !string.IsNullOrWhiteSpace(p.CondicoesMedicas))
            .Select(p => p.CondicoesMedicas)
            .Distinct()
            .ToList();

        if (!doencas.Any())
        {
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
        IsFiltroAberto = false;
        AplicarFiltros();
    }

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