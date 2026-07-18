using System.Collections.ObjectModel;
using CareConnect.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Shared;

public partial class UtentesViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Utente> _listaUtentes = new();

    // 🔒 LÓGICA DE PERMISSÕES: Devolve true se for Gestor, false se for Cuidador.
    public bool IsGestor => Preferences.Default.Get("auth_profile", "Gestor") == "Gestor";

    public UtentesViewModel()
    {
        
    }

    [RelayCommand]
    private void CarregarUtentesAsync()
    {
        // Simulando a API com os dados do teu design
        ListaUtentes = new ObservableCollection<Utente>
        {
            new Utente { Nome = "Maria Da Conceição Silva", Idade = 78, NomeCuidador = "Ana Silva", StatusCuidado = "Estável", FotoUrl = "avatar_1.png" },
            new Utente { Nome = "João Mendes", Idade = 82, NomeCuidador = "Ana Silva", StatusCuidado = "Alerta", FotoUrl = "avatar_2.png" },
            new Utente { Nome = "Carla Rodrigues", Idade = 75, NomeCuidador = "Marcos Dias", StatusCuidado = "Estável", FotoUrl = "avatar_3.png" }
        };
    }

    [RelayCommand]
    private async Task VerDetalhesAsync(Utente utenteSelecionado)
    {
        if (utenteSelecionado == null) return;

        // Prepara o utente selecionado na "mochila" para a página de Detalhes
        var parametros = new Dictionary<string, object>
        {
            { "UtenteSelecionado", utenteSelecionado }
        };

        // Navega para a página de detalhes partilhada
        await Shell.Current.GoToAsync("DetalheUtenteView", parametros);
    }

    [RelayCommand]
    private async Task AdicionarUtenteAsync()
    {
        // Proteção dupla: mesmo que um cuidador consiga forçar o clique, o código bloqueia
        if (!IsGestor) return;

        await Shell.Current.GoToAsync("AdicionarUtenteView");
    }
}