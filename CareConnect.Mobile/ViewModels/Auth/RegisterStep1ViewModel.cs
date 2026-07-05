using System.Text.RegularExpressions;
using CareConnect.Mobile.Services;
using CareConnect.Mobile.Shells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth;

public partial class RegisterStep1ViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string _nomeCompleto = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _perfilSelecionado = "Gestor"; 

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _fotoCaminho = "avatar_placeholder.png";

    private string _caminhoArquivoReal = string.Empty;

    public RegisterStep1ViewModel(AuthService authService, INotificationService notificationService)
    {
        _authService = authService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    private async Task RegistarAsync()
    {
        NomeCompleto = NomeCompleto?.Trim() ?? string.Empty;
        Email = Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(NomeCompleto) || 
            string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Password))
        {
            await _notificationService.MostrarAvisoAsync("Por favor, preencha todos os campos.");
            return;
        }

        if (string.IsNullOrEmpty(_caminhoArquivoReal) || FotoCaminho == "avatar_placeholder.png")
        {
            await _notificationService.MostrarAvisoAsync("Por favor, selecione uma foto de perfil para continuar.");
            return;
        }

        var nomeRegex = new Regex(@"^[a-zA-ZÀ-ÿ\s]+$");
        if (!nomeRegex.IsMatch(NomeCompleto))
        {
            await _notificationService.MostrarAvisoAsync("O nome contém caracteres inválidos. Use apenas letras.");
            return;
        }

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(Email))
        {
            await _notificationService.MostrarAvisoAsync("Por favor, insira um endereço de e-mail válido.");
            return;
        }

        if (Password.Length < 6)
        {
            await _notificationService.MostrarAvisoAsync("A password deve ter pelo menos 6 caracteres.");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await _notificationService.MostrarAvisoAsync("As passwords não coincidem.");
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            { "Nome", NomeCompleto },
            { "Email", Email },
            { "Password", Password },
            { "FotoCaminho", _caminhoArquivoReal }
        };

        await Shell.Current.GoToAsync("ProfileSelectionView", parametros);
    }

    [RelayCommand]
    private async Task EscolherFotoAsync()
    {
        try
        {
            var foto = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Selecione a sua foto de perfil" });
            if (foto != null)
            {
                _caminhoArquivoReal = foto.FullPath;
                FotoCaminho = foto.FullPath; // Atualiza o ecrã
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync("Erro ao selecionar foto: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task GoBackToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}