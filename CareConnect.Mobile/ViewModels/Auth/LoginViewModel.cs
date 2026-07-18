using CareConnect.Mobile.Services;
using CareConnect.Mobile.Shells; // Assumindo que as tuas Shells estão nesta pasta
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))] // Dica pro: cria a propriedade inversa automaticamente!
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public LoginViewModel(AuthService authService, INotificationService notificationService)
        {
            _authService = authService;
            _notificationService = notificationService;

            // PRÉ-PREENCHIMENTO: Puxa o último e-mail logado (se existir)
            Email = _authService.ObterUltimoEmail();
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            Email = Email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await _notificationService.MostrarAvisoAsync("Por favor, preencha todos os campos.");
                return;
            }

            try
            {
                IsLoading = true;
                var resposta = await _authService.LoginAsync(Email, Password);

                if (resposta.Sucesso)
                {
                    await _notificationService.MostrarSucessoAsync("Sessão iniciada com sucesso!");

                    // Redireciona para o ecossistema correto com base no Role do utilizador
                    if (resposta.Perfil == "Gestor")
                    {
                        Application.Current!.Windows[0].Page = new GestorShell();
                    }
                    else
                    {
                        Application.Current!.Windows[0].Page = new CuidadorShell();
                    }
                }
                else
                {
                    // Mostra o erro devolvido pela API (ex: "Palavra-passe incorreta." ou "Utilizador não encontrado.")
                    await _notificationService.MostrarErroAsync(resposta.MensagemErro);
                }
                IsLoading = false;
            }
            finally
            {
                IsLoading = false;
            }
            
        }

        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("RegisterView");
        }

        [RelayCommand]
        private async Task GoToForgotPasswordAsync()
        {
            await Shell.Current.GoToAsync("ForgotPasswordView");
        }
    }
}