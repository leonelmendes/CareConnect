using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Mobile.Services;

namespace CareConnect.Mobile.ViewModels.Shared;

public partial class PerfilViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string _nome = "Carregando...";

    [ObservableProperty]
    private string _emaill = "---";

    [ObservableProperty]
    private string _perfil = "Gestor";

    [ObservableProperty]
    private string _avatarUrl = "avatar_elderly.png"; // Fallback por defeito

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private bool _isGestor;

    public PerfilViewModel(AuthService authService, INotificationService notificationService)
    {
        _authService = authService;
        CarregarDadosPerfil();
        _notificationService = notificationService;
    }

    public void CarregarDadosPerfil()
    {
        // 1. Lê os dados salvos nas Preferências
        Nome = Preferences.Default.Get("user_nome", "Utilizador CareConnect");
        Emaill = Preferences.Default.Get("last_logged_email", "email@careconnect.pt");


        Perfil = Preferences.Default.Get("auth_profile", "Cuidador");
        IsGestor = Perfil.Equals("Gestor", StringComparison.OrdinalIgnoreCase);

        var fotoSalva = Preferences.Default.Get("user_avatar", string.Empty);
        if (!string.IsNullOrWhiteSpace(fotoSalva))
        {
            AvatarUrl = fotoSalva;
        }

        // 2. Verifica qual é o tema atual da App para ajustar o Switch
        var temaSalvo = Preferences.Default.Get("app_theme_dark", false);
        _isDarkMode = temaSalvo;
        OnPropertyChanged(nameof(IsDarkMode));
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        Preferences.Default.Set("app_theme_dark", value);

        // Altera o tema da aplicação em tempo real!
        Application.Current!.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
    }

    [RelayCommand]
    private async Task TerminarSessaoAsync()
    {
        bool confirmar = await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Terminar Sessão", 
            "Tem a certeza que deseja sair da sua conta?", 
            "Sim, Sair", "Cancelar");

        if (confirmar)
        {
            _authService.FazerLogout();
            Application.Current!.Windows[0].Page = new AppShell();
        }
    }

    [RelayCommand]
    private async Task NavegarMenuAsync(string menu)
    {
        // Podes adicionar navegação real para cada opção de perfil mais tarde
        await Shell.Current.DisplayAlert(menu, $"Ecrã '{menu}' em construção.", "OK");
    }
}