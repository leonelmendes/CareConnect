using CareConnect.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth
{
    [QueryProperty(nameof(NomeRecebido), "Nome")]
    [QueryProperty(nameof(EmailRecebido), "Email")]
    [QueryProperty(nameof(PasswordRecebida), "Password")]
    public partial class ProfileSelectionViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        public ProfileSelectionViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [ObservableProperty]
        private string _selectedProfile = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public string NomeRecebido { get; set; } = string.Empty;
        public string EmailRecebido { get; set; } = string.Empty;
        public string PasswordRecebida { get; set; } = string.Empty;

        [RelayCommand]
        private async Task ContinueAsync()
        {
            if (string.IsNullOrEmpty(SelectedProfile))
            {
                await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Aviso", "Por favor, selecione um perfil para continuar.", "OK");
                return;
            }

            IsLoading = true;

            var resposta = await _authService.RegistarAsync(NomeRecebido, EmailRecebido, PasswordRecebida, SelectedProfile);

            IsLoading = false;

            if (resposta.Sucesso)
            {
                await Shell.Current.GoToAsync($"//OnboardingView?Perfil={SelectedProfile}");
            }
            else
            {
                await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Erro no Registo", resposta.MensagemErro, "OK");
            }
        }
    }
}