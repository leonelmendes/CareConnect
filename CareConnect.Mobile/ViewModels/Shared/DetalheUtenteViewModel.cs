using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CareConnect.Shared.Models;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services;
using CommunityToolkit.Mvvm.Messaging;
using CareConnect.Mobile.Messages;

namespace CareConnect.Mobile.ViewModels.Shared;

[QueryProperty(nameof(UtenteRecebido), "UtenteSelecionado")]
public partial class DetalheUtenteViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly PatientService _patientService;

    public bool IsGestor => Preferences.Default.Get("auth_profile", "Gestor") == "Gestor";

    [ObservableProperty]
    private Patient _utenteRecebido;

    [ObservableProperty]
    private string _idadeFormatada;

    [ObservableProperty]
    private ObservableCollection<CondicaoMedica> _condicoes = new();

    [ObservableProperty]
    private ObservableCollection<CuidadorAtribuido> _cuidadores = new();

    public DetalheUtenteViewModel(INotificationService service, PatientService patientService)
    {
        _notificationService = service;
        _patientService = patientService;

        // ADEUS MOCK DATA! Foi totalmente removido.

        WeakReferenceMessenger.Default.Register<PatientUpdatedMessage>(this, (r, m) =>
        {
            if (UtenteRecebido != null && UtenteRecebido.Id == m.Value.Id)
            {
                UtenteRecebido = m.Value;
                OnPropertyChanged(nameof(UtenteRecebido));
                OnUtenteRecebidoChanged(UtenteRecebido);
            }
        });
    }

    partial void OnUtenteRecebidoChanged(Patient value)
    {
        if (value != null)
        {
            // 1. Calcular Idade
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

            // 2. Gerar Cartões das Condições (mantivemos a tua excelente lógica)
            GerarCartoesCondicoes(value.CondicoesMedicas);

            // 3. NOVO: Gerar Cartões dos Cuidadores Reais associados ao Paciente
            GerarCartoesCuidadores(value.Cuidadores);
        }
    }

    private void GerarCartoesCuidadores(ICollection<User> cuidadoresDaApi)
    {
        Cuidadores.Clear();

        if (cuidadoresDaApi == null || !cuidadoresDaApi.Any()) return;

        foreach (var cuidador in cuidadoresDaApi)
        {
            Cuidadores.Add(new CuidadorAtribuido
            {
                Nome = cuidador.Nome,
                // Assumindo que a classe User tem a propriedade Role (Enum), converte para texto.
                // Se o teu modelo tiver algo diferente, ajusta aqui.
                Cargo = "Cuidador",
                FotoUrl = string.IsNullOrWhiteSpace(cuidador.AvatarUrl) ? "avatar_elderly.png" : cuidador.AvatarUrl
            });
        }
    }

    private void GerarCartoesCondicoes(string condicoesTexto)
    {
        Condicoes.Clear();

        if (string.IsNullOrWhiteSpace(condicoesTexto)) return;

        var doencasArray = condicoesTexto.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var doencaTexto in doencasArray)
        {
            var nomeLimpo = doencaTexto.Trim();

            Condicoes.Add(new CondicaoMedica
            {
                Icone = ObterIconeParaDoenca(nomeLimpo),
                Nome = char.ToUpper(nomeLimpo[0]) + nomeLimpo.Substring(1),
                DataDiagnostico = "Data não especificada",
                Status = "Registado"
            });
        }
    }

    private string ObterIconeParaDoenca(string doenca)
    {
        var d = doenca.ToLower();
        if (d.Contains("diabet")) return "🩸";
        if (d.Contains("hipertensão") || d.Contains("coração") || d.Contains("card")) return "🫀";
        if (d.Contains("respira") || d.Contains("asma") || d.Contains("dpoc")) return "🫁";
        if (d.Contains("alzheimer") || d.Contains("demência") || d.Contains("neuro")) return "🧠";
        if (d.Contains("osso") || d.Contains("artrite") || d.Contains("osteo")) return "🦴";
        return "🩺";
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

        string primeiroNome = UtenteRecebido.Nome?.Split(' ')[0] ?? "Utente";
        string fraseValidacao = $"Deletar {primeiroNome}";

        string resultado = await Shell.Current.DisplayPromptAsync(
            "Eliminar Utente",
            $"Esta ação é irreversível. Para confirmar, digite exatamente: {fraseValidacao}",
            "Eliminar",
            "Cancelar",
            placeholder: fraseValidacao);

        if (resultado == fraseValidacao)
        {
            bool sucesso = await _patientService.DeletePatientAsync(UtenteRecebido.Id);
            if (sucesso)
            {
                await _notificationService.MostrarAvisoAsync("Utente eliminado com sucesso.");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await _notificationService.MostrarAvisoAsync("Erro ao comunicar com o servidor.");
            }
        }
        else if (!string.IsNullOrEmpty(resultado))
        {
            await _notificationService.MostrarAvisoAsync("A frase de segurança não coincide. Ação cancelada.");
        }
    }
}