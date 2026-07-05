using CareConnect.Mobile.Services;
using CareConnect.Mobile.Views.Auth; // Para redirecionar para o Login
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Shared;

public partial class PerfilViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string _nome = string.Empty;

    [ObservableProperty]
    private string _cargo = string.Empty;

    [ObservableProperty]
    private string _fotoUrl = "avatar_1.png"; // Placeholder da foto

    // Lógica de Permissões
    public bool IsGestor => Preferences.Default.Get("auth_profile", "Gestor") == "Gestor";

    public PerfilViewModel(AuthService authService, INotificationService notificationService)
    {
        _authService = authService;
        _notificationService = notificationService;
        CarregarDadosUsuario();
    }

    private void CarregarDadosUsuario()
    {
        // Vai buscar o Nome e o Perfil (Cargo) às Preferences
        Nome = Preferences.Default.Get("auth_name", "Carla Monteiro"); // Usa um fallback do design

        var perfilStr = Preferences.Default.Get("auth_profile", "Gestor");
        Cargo = perfilStr == "Gestor" ? "Gestor de Cuidados" : "Cuidador Principal";
    }

    [RelayCommand]
    private async Task NavegarMenuAsync(string menu)
    {
        // Exemplo usando a nossa notificação em vez do DisplayAlert!
        await _notificationService.MostrarAvisoAsync($"A abrir a secção: {menu}...");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        // Limpa todos os dados da sessão (Cofre e Preferences)
        _authService.FazerLogout();

        await _notificationService.MostrarSucessoAsync("Sessão terminada com sucesso.");

        // Redireciona para o ecrã de Login e limpa o histórico da Shell!
        Application.Current!.Windows[0].Page = new AppShell();
    }
}