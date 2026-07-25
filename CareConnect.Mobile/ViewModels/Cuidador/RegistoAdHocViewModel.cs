using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// Ajusta os namespaces consoante a tua estrutura
// using CareConnect.Mobile.Models; 
// using CareConnect.Mobile.Services;

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class RegistoAdHocViewModel : ObservableObject
{
    // PROPRIEDADES LIGADAS À INTERFACE (XAML)
    
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

    public RegistoAdHocViewModel()
    {
        UtentesDisponiveis = new ObservableCollection<UtenteResumo>();
        
        // Num cenário real, aqui chamarias um _utenteService.ObterUtentesAsync()
        // Para já, vamos colocar dados de teste para veres o Picker a funcionar:
        CarregarUtentesDeTeste();
    }

    private void CarregarUtentesDeTeste()
    {
        UtentesDisponiveis.Add(new UtenteResumo { Id = 1, Nome = "Maria Silva" });
        UtentesDisponiveis.Add(new UtenteResumo { Id = 2, Nome = "João Santos" });
    }

    [RelayCommand]
    private async Task GuardarAdHocAsync()
    {
        // 1. Validações básicas
        if (UtenteSelecionado == null)
        {
            await Shell.Current.DisplayAlert("Aviso", "Por favor, selecione um utente.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(TituloTarefa))
        {
            await Shell.Current.DisplayAlert("Aviso", "Por favor, indique o que foi feito.", "OK");
            return;
        }

        IsBusy = true;

        try
        {
            // 2. Aqui no futuro vais chamar a tua API para guardar
            // Exemplo: await _tarefaService.RegistarAdHocAsync(novoRegisto);
            
            // Simular o tempo de resposta da API
            await Task.Delay(1000); 

            // 3. Feedback de sucesso e voltar ao ecrã anterior
            await Shell.Current.DisplayAlert("Sucesso", "Registo Ad-Hoc guardado com sucesso!", "OK");
            
            // Voltar ao Dashboard
            await Shell.Current.GoToAsync(".."); 
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Não foi possível guardar o registo: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

// Classe auxiliar simples (podes usar a que já tiveres no teu projeto)
public class UtenteResumo
{
    public int Id { get; set; }
    public string Nome { get; set; }
}