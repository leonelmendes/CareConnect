using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services; // Importa os modelos criados

namespace CareConnect.Mobile.ViewModels.Shared; // Pasta partilhada!

[QueryProperty(nameof(UtenteRecebido), "UtenteSelecionado")]
public partial class DetalheUtenteViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    // 🔒 LÓGICA DE PERMISSÕES: Controla o que o utilizador pode ver/fazer
    public bool IsGestor => Preferences.Default.Get("auth_profile", "Gestor") == "Gestor";

    // O utente que foi clicado no ecrã anterior (Lista) é injetado aqui automaticamente
    [ObservableProperty]
    private Utente _utenteRecebido;

    [ObservableProperty]
    private ObservableCollection<CondicaoMedica> _condicoes = new();

    [ObservableProperty]
    private ObservableCollection<CuidadorAtribuido> _cuidadores = new();

    public DetalheUtenteViewModel(INotificationService service)
    {
        CarregarMockData();
        _notificationService = service;
    }

    // Método acionado automaticamente pelo MAUI quando o UtenteRecebido é preenchido
    partial void OnUtenteRecebidoChanged(Utente value)
    {
        if (value != null)
        {
            // Opcional no futuro: Buscar histórico médico à API usando o value.Id
        }
    }

    private void CarregarMockData()
    {
        Condicoes = new ObservableCollection<CondicaoMedica>
        {
            new CondicaoMedica { Icone = "🩸", Nome = "Diabetes Tipo 2", DataDiagnostico = "Mai 10, 2015", Status = "Controlado" },
            new CondicaoMedica { Icone = "🫀", Nome = "Hipertensão", DataDiagnostico = "Ago 22, 2012", Status = "Controlado" },
            new CondicaoMedica { Icone = "🫁", Nome = "DPOC", DataDiagnostico = "Jan 5, 2018", Status = "Estável" }
        };

        Cuidadores = new ObservableCollection<CuidadorAtribuido>
        {
            new CuidadorAtribuido { Nome = "Sarah Miller", Cargo = "Enf. Principal", FotoUrl = "avatar_1.png" },
            new CuidadorAtribuido { Nome = "Michael Brown", Cargo = "Assistente", FotoUrl = "dotnet_bot.png" }
        };
    }

    [RelayCommand]
    private async Task LigarEmergenciaAsync()
    {
        await _notificationService.MostrarAvisoAsync("A iniciar chamada para o 112 ou contacto de emergência...");
    }
}