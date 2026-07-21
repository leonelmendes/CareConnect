using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CareConnect.Shared.Models; // Usar o Patient da API
using CareConnect.Mobile.Models; // Para as tuas mocks de CondicaoMedica/Cuidador
using CareConnect.Mobile.Services;
using CommunityToolkit.Mvvm.Messaging;
using CareConnect.Mobile.Messages;

namespace CareConnect.Mobile.ViewModels.Shared;

// Recebe o Patient que foi passado pelo clique na UtentesView
[QueryProperty(nameof(UtenteRecebido), "UtenteSelecionado")]
public partial class DetalheUtenteViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly PatientService _patientService; // Injetamos o serviço da API

    public bool IsGestor => Preferences.Default.Get("auth_profile", "Gestor") == "Gestor";

    // 1. Mudamos o tipo de Utente para Patient
    [ObservableProperty]
    private Patient _utenteRecebido;
    [ObservableProperty]
    private string _idadeFormatada;

    [ObservableProperty]
    private ObservableCollection<CondicaoMedica> _condicoes = new();

    [ObservableProperty]
    private ObservableCollection<CuidadorAtribuido> _cuidadores = new();

    // 2. Construtor recebe o PatientService
    public DetalheUtenteViewModel(INotificationService service, PatientService patientService)
    {
        _notificationService = service;
        _patientService = patientService;
        CarregarMockData();

        WeakReferenceMessenger.Default.Register<PatientUpdatedMessage>(this, (r, m) =>
        {
            // Se o utente atualizado for o mesmo que estamos a ver neste ecrã...
            if (UtenteRecebido != null && UtenteRecebido.Id == m.Value.Id)
            {
                UtenteRecebido = m.Value;
                
                // Força o XAML a atualizar os visuais na hora!
                OnPropertyChanged(nameof(UtenteRecebido));
                
                // Recalcula a idade com a nova data
                OnUtenteRecebidoChanged(UtenteRecebido);
            }
        });
    }

    // 3. Atualizado para Patient
    partial void OnUtenteRecebidoChanged(Patient value)
    {
        if (value != null)
        {
            // 1. Calcula a idade (mantém o que já tinhas feito)
            if (value.DataNascimento != default && value.DataNascimento != DateTime.MinValue)
            {
                var hoje = DateTime.Today;
                var idade = hoje.Year - value.DataNascimento.Year;
                if (value.DataNascimento.Date > hoje.AddYears(-idade)) idade--;
                IdadeFormatada = $"{idade} anos ({value.DataNascimento:dd/MM/yyyy})";
            }
            else
            {
                IdadeFormatada = "Data de nascimento não registada";
            }

            // 2. Transforma a string de condições numa lista de cartões visuais
            GerarCartoesCondicoes(value.CondicoesMedicas);
        }
    }

    private void GerarCartoesCondicoes(string condicoesTexto)
    {
        Condicoes.Clear(); // Limpa as condições anteriores

        if (string.IsNullOrWhiteSpace(condicoesTexto)) return;

        // Parte a string usando a vírgula (ou ponto e vírgula) como separador
        var doencasArray = condicoesTexto.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var doencaTexto in doencasArray)
        {
            var nomeLimpo = doencaTexto.Trim();
            
            Condicoes.Add(new CondicaoMedica
            {
                Icone = ObterIconeParaDoenca(nomeLimpo),
                Nome = char.ToUpper(nomeLimpo[0]) + nomeLimpo.Substring(1), // Capitaliza a primeira letra
                DataDiagnostico = "Data não especificada", // A API não fornece data
                Status = "Registado" // Status padrão
            });
        }
    }

    // Um pequeno dicionário de ícones automático
    private string ObterIconeParaDoenca(string doenca)
    {
        var d = doenca.ToLower();
        if (d.Contains("diabet")) return "🩸";
        if (d.Contains("hipertensão") || d.Contains("coração") || d.Contains("card")) return "🫀";
        if (d.Contains("respira") || d.Contains("asma") || d.Contains("dpoc")) return "🫁";
        if (d.Contains("alzheimer") || d.Contains("demência") || d.Contains("neuro")) return "🧠";
        if (d.Contains("osso") || d.Contains("artrite") || d.Contains("osteo")) return "🦴";
        return "🩺"; // Ícone genérico de medicina
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
    private async Task LigarAsync()
    {
        if (UtenteRecebido == null || string.IsNullOrWhiteSpace(UtenteRecebido.Contacto))
        {
            await _notificationService.MostrarAvisoAsync("Contacto principal não registado para este utente.");
            return;
        }

        if (PhoneDialer.Default.IsSupported)
            PhoneDialer.Default.Open(UtenteRecebido.Contacto);
        else
            await _notificationService.MostrarAvisoAsync("Este dispositivo não suporta chamadas telefónicas.");
    }

    [RelayCommand]
    private async Task LigarEmergenciaAsync()
    {
        if (UtenteRecebido == null || string.IsNullOrWhiteSpace(UtenteRecebido.ContactoEmergencia))
        {
            await _notificationService.MostrarAvisoAsync("Contacto de emergência não registado para este utente.");
            return;
        }

        if (PhoneDialer.Default.IsSupported)
            PhoneDialer.Default.Open(UtenteRecebido.ContactoEmergencia);
        else
            await _notificationService.MostrarAvisoAsync("Este dispositivo não suporta chamadas telefónicas.");
    }

    [RelayCommand]
    private async Task EditarUtenteAsync()
    {
        if (!IsGestor || UtenteRecebido == null) return;
        
        // Passa o utente atual para a próxima página de edição
        var parametros = new Dictionary<string, object>
        {
            { "UtenteEditar", UtenteRecebido }
        };
        await Shell.Current.GoToAsync("EditarUtenteView", parametros);
    }

    [RelayCommand]
    private async Task DeletarUtenteAsync()
    {
        if (!IsGestor || UtenteRecebido == null) return;

        // Extrai apenas o primeiro nome para facilitar a digitação
        string primeiroNome = UtenteRecebido.Nome?.Split(' ')[0] ?? "Utente";
        string fraseValidacao = $"Deletar {primeiroNome}";

        // Pede ao utilizador para digitar a frase exata
        string resultado = await Shell.Current.DisplayPromptAsync(
            "Eliminar Utente",
            $"Esta ação é irreversível. Para confirmar, digite exatamente: {fraseValidacao}",
            "Eliminar",
            "Cancelar",
            placeholder: fraseValidacao);

        if (resultado == fraseValidacao)
        {
            // NOTA: Garante que tens um método DeletePatientAsync(Guid id) no teu PatientService!
            bool sucesso = await _patientService.DeletePatientAsync(UtenteRecebido.Id);
            if (sucesso)
            {
                await _notificationService.MostrarAvisoAsync("Utente eliminado com sucesso.");
                await Shell.Current.GoToAsync(".."); // Volta para a lista
            }
            else
            {
                await _notificationService.MostrarAvisoAsync("Erro ao comunicar com o servidor.");
            }

            // Código temporário até o método da API estar conectado:
            await _notificationService.MostrarAvisoAsync($"{UtenteRecebido.Nome} apagado (Simulação).");
            await Shell.Current.GoToAsync("..");
        }
        else if (!string.IsNullOrEmpty(resultado))
        {
            await _notificationService.MostrarAvisoAsync("A frase de segurança não coincide. Ação cancelada.");
        }
    }
}