using CareConnect.Mobile.Services;
using CareConnect.Mobile.Views.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth
{
    [QueryProperty(nameof(NomeRecebido), "Nome")]
    [QueryProperty(nameof(EmailRecebido), "Email")]
    [QueryProperty(nameof(PasswordRecebida), "Password")]
    [QueryProperty(nameof(FotoRecebida), "FotoCaminho")]
    public partial class ProfileSelectionViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly NotificationService _notificationService;

        public ProfileSelectionViewModel(AuthService authService, NotificationService notificationService)
        {
            _authService = authService;
            _notificationService = notificationService;
        }

        [ObservableProperty]
        private string _selectedProfile = string.Empty;

        //[ObservableProperty]
        //private string _fotoRecebida = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public string NomeRecebido { get; set; } = string.Empty;
        public string EmailRecebido { get; set; } = string.Empty;
        public string PasswordRecebida { get; set; } = string.Empty;
        public string FotoRecebida { get; set; } = string.Empty;

        [RelayCommand]
        private async Task ContinueAsync()
        {
            if (string.IsNullOrEmpty(SelectedProfile))
            {
                await _notificationService.MostrarAvisoAsync("Por favor, selecione um perfil para continuar.");
                return;
            }

            IsLoading = true;

            var resposta = await _authService.RegistarAsync(NomeRecebido, EmailRecebido, PasswordRecebida, SelectedProfile);

            if (resposta.Sucesso)
            {
                var respostaLogin = await _authService.LoginAsync(EmailRecebido, PasswordRecebida);
                IsLoading = false;

                if (respostaLogin.Sucesso)
                {
                    if (!string.IsNullOrEmpty(FotoRecebida))
                    {
                       await _authService.UploadAvatarAsync(FotoRecebida);
                    }
                    await _notificationService.MostrarSucessoAsync("Conta criada e sessão iniciada!");
                    await Shell.Current.GoToAsync($"//OnboardingView?Perfil={SelectedProfile}");
                }
                else
                {
                    await _notificationService.MostrarAvisoAsync("Conta criada! Por favor, inicie sessão.");
                    Application.Current!.Windows[0].Page = new AppShell();
                }

            }
            else
            {
                IsLoading = false;
                await _notificationService.MostrarErroAsync("Erro no Registo");
            }
        }
    }
}