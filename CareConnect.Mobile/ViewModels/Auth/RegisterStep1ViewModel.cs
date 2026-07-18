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
    private ImageSource _fotoPerfil = "avatar_placeholder.png";

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

        // 1. Validação de campos vazios
        if (string.IsNullOrWhiteSpace(NomeCompleto) || 
            string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Password))
        {
            await _notificationService.MostrarAvisoAsync("Por favor, preencha todos os campos.");
            return;
        }

        // 2. VALIDAÇÃO DA FOTO (Atualizada para ImageSource):
        // Como o _caminhoArquivoReal só é preenchido no EscolherFotoAsync, basta verificá-lo!
        if (string.IsNullOrEmpty(_caminhoArquivoReal))
        {
            await _notificationService.MostrarAvisoAsync("Por favor, selecione uma foto de perfil para continuar.");
            return;
        }

        // 3. Validação do Nome (Apenas letras)
        var nomeRegex = new Regex(@"^[a-zA-ZÀ-ÿ\s]+$");
        if (!nomeRegex.IsMatch(NomeCompleto))
        {
            await _notificationService.MostrarAvisoAsync("O nome contém caracteres inválidos. Use apenas letras.");
            return;
        }

        // 4. Validação do E-mail
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(Email))
        {
            await _notificationService.MostrarAvisoAsync("Por favor, insira um endereço de e-mail válido.");
            return;
        }

        // 5. Validação de Segurança da Password
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

        // 6. Tudo válido! Preparamos a "mochila" para o próximo ecrã
        var parametros = new Dictionary<string, object>
        {
            { "Nome", NomeCompleto },
            { "Email", Email },
            { "Password", Password },
            { "FotoCaminho", _caminhoArquivoReal } // Passamos o caminho real para fazer o upload na próxima tela
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
                // 1. Abre o fluxo de leitura do ficheiro temporário do sistema
                using var streamOrigem = await foto.OpenReadAsync();

                // 2. Cria um nome único e um caminho na pasta de Cache da nossa App (Onde o iOS nunca bloqueia!)
                var extensao = Path.GetExtension(foto.FileName);
                var nomeArquivoCache = $"avatar_{Guid.NewGuid()}{extensao}";
                var caminhoCache = Path.Combine(FileSystem.Current.CacheDirectory, nomeArquivoCache);

                // 3. Copia fisicamente a foto para a nossa Cache
                using var streamDestino = File.Create(caminhoCache);
                await streamOrigem.CopyToAsync(streamDestino);
                streamDestino.Close();

                // 4. Agora sim! Guardamos o caminho seguro da nossa cache
                _caminhoArquivoReal = caminhoCache;

                // 5. Atualizamos a UI sem engasgos (funciona perfeito no iOS e Android)
                FotoPerfil = ImageSource.FromFile(caminhoCache);
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