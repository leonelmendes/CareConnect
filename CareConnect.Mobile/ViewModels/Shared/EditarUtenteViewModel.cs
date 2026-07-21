using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Shared.Models;
using CareConnect.Mobile.Services;
using CommunityToolkit.Mvvm.Messaging;
using CareConnect.Mobile.Messages;

namespace CareConnect.Mobile.ViewModels.Shared;

[QueryProperty(nameof(UtenteEditar), "UtenteEditar")]
public partial class EditarUtenteViewModel : ObservableObject
{
    private readonly PatientService _patientService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private Patient _utenteEditar;

    // Propriedades do Formulário atualizadas com o modelo da API
    [ObservableProperty] private string _nome;
    [ObservableProperty] private DateTime _dataNascimento = DateTime.Today;
    [ObservableProperty] private string _contacto;
    [ObservableProperty] private string _contactoEmergencia;
    [ObservableProperty] private string _condicoesMedicas;
    private FileResult _novaFotoSelecionada;

    [ObservableProperty] 
    private ImageSource _avatarPreview;
    
    // NOVOS CAMPOS
    [ObservableProperty] private string _alergias;
    [ObservableProperty] private string _notas;
    [ObservableProperty] private bool _isBusy;

    public EditarUtenteViewModel(PatientService patientService, INotificationService notificationService)
    {
        _patientService = patientService;
        _notificationService = notificationService;
    }

    partial void OnUtenteEditarChanged(Patient value)
    {
        if (value != null)
        {
            // 2. Se a imagem vier da API (https), forçamos o carregamento via URI
            if (!string.IsNullOrEmpty(value.AvatarUrl))
            {
                if (value.AvatarUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    AvatarPreview = ImageSource.FromUri(new Uri(value.AvatarUrl));
                }
                else
                {
                    AvatarPreview = ImageSource.FromFile(value.AvatarUrl);
                }
            }

            Nome = value.Nome;
            DataNascimento = value.DataNascimento != default ? value.DataNascimento : DateTime.Today;
            Contacto = value.Contacto;
            ContactoEmergencia = value.ContactoEmergencia;
            CondicoesMedicas = value.CondicoesMedicas;
            Alergias = value.Alergias;
            Notas = value.Notas;
        }
    }

    [RelayCommand]
    private async Task AlterarFotoAsync()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var fotoResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecione uma foto de perfil"
                });

                if (fotoResult != null)
                {
                    _novaFotoSelecionada = fotoResult;
                    
                    // 1. Abre o ficheiro selecionado
                    using var stream = await fotoResult.OpenReadAsync();
                    
                    // 2. Copia para a memória da nossa app (evita bloqueios do Android)
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // Volta ao início para a imagem poder ser lida

                    // 3. Atualiza a UI na Thread principal usando a memória
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AvatarPreview = ImageSource.FromStream(() => memoryStream);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarAvisoAsync("Ação cancelada ou ocorreu um erro.");
        }
    }

    [RelayCommand]
    private async Task GuardarAlteracoesAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            await _notificationService.MostrarAvisoAsync("O nome é obrigatório.");
            return;
        }

        IsBusy = true;

        // SE o utilizador escolheu uma foto nova, fazemos o upload
        if (_novaFotoSelecionada != null)
        {
            var novaUrlS3 = await _patientService.UploadFotoPerfilAsync(UtenteEditar.Id, _novaFotoSelecionada);
            
            // VALIDAÇÃO RIGOROSA:
            if (string.IsNullOrEmpty(novaUrlS3))
            {
                IsBusy = false;
                await _notificationService.MostrarAvisoAsync("Erro: O servidor não aceitou a imagem ou não devolveu o novo link.");
                return; // Pára aqui, não avança para guardar o resto!
            }

            // TRUQUE DE CACHE: Adicionar um carimbo de tempo ao URL engana o MAUI 
            // e obriga-o a descarregar a imagem nova da AWS S3
            var timestamp = DateTime.UtcNow.Ticks;
            UtenteEditar.AvatarUrl = $"{novaUrlS3}?v={timestamp}"; 
        }

        // ... (resto do código continua igual)
        UtenteEditar.Nome = Nome;
        UtenteEditar.DataNascimento = DataNascimento;
        UtenteEditar.Contacto = Contacto;
        UtenteEditar.ContactoEmergencia = ContactoEmergencia;
        UtenteEditar.CondicoesMedicas = CondicoesMedicas;
        UtenteEditar.Alergias = Alergias;
        UtenteEditar.Notas = Notas;

        // Envia o JSON final atualizado para a API
        bool sucesso = await _patientService.UpdatePatientAsync(UtenteEditar);
        
        IsBusy = false;

        if (sucesso)
        {
            await _notificationService.MostrarAvisoAsync("Utente atualizado com sucesso!");
            WeakReferenceMessenger.Default.Send(new PatientUpdatedMessage(UtenteEditar));
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("..");
            });
        }
        else
        {
            await _notificationService.MostrarAvisoAsync("Erro ao atualizar o utente.");
        }
    }
    
    [RelayCommand]
    private async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}