using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Shared.Models;
using CareConnect.Mobile.Services;

namespace CareConnect.Mobile.ViewModels.Gestor;

public partial class SelecaoUtenteRelatorioViewModel : ObservableObject
{
    private readonly PatientService _patientService;

    public ObservableCollection<Patient> Utentes { get; } = new();

    public SelecaoUtenteRelatorioViewModel(PatientService patientService)
    {
        _patientService = patientService;
        _ = CarregarUtentesAsync();
    }

    private async Task CarregarUtentesAsync()
    {
        var lista = await _patientService.GetMyPatientsAsync();
        Utentes.Clear();
        foreach (var u in lista) Utentes.Add(u);
    }

    [RelayCommand]
    private async Task UtenteSelecionadoAsync(Patient utenteSelecionado)
    {
        if (utenteSelecionado == null) return;

        // A forma mais segura de passar parâmetros no MAUI (evita crashes com espaços ou acentos)
        var parametros = new Dictionary<string, object>
    {
        { "UtenteId", utenteSelecionado.Id.ToString() },
        { "NomeUtente", utenteSelecionado.Nome }
    };

        // Navegamos passando o dicionário em vez de concatenar uma string enorme
        await Shell.Current.GoToAsync("RelatorioDiarioView", parametros);
    }
}