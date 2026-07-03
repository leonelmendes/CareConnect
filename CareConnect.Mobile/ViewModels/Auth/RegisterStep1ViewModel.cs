using System.Text.RegularExpressions;
using CareConnect.Mobile.Services;
using CareConnect.Mobile.Shells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth;

public partial class RegisterStep1ViewModel : ObservableObject
{
    private readonly AuthService _authService;
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

    public RegisterStep1ViewModel(AuthService authService)
    {
        _authService = authService;
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
            await MostrarErro("Por favor, preencha todos os campos.");
            return;
        }

        var nomeRegex = new Regex(@"^[a-zA-ZÀ-ÿ\s]+$");
        if (!nomeRegex.IsMatch(NomeCompleto))
        {
            await MostrarErro("O nome contém caracteres inválidos. Use apenas letras.");
            return;
        }

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(Email))
        {
            await MostrarErro("Por favor, insira um endereço de e-mail válido.");
            return;
        }

        if (Password.Length < 6)
        {
            await MostrarErro("A password deve ter pelo menos 6 caracteres.");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await MostrarErro("As passwords não coincidem.");
            return;
        }

        IsLoading = true;

        var resposta = await _authService.RegistarAsync(NomeCompleto, Email, Password, PerfilSelecionado);

        IsLoading = false;

        if (resposta.Sucesso)
        {
            var parametros = new Dictionary<string, object>
            {
                { "Nome", NomeCompleto },
                { "Email", Email },
                { "Password", Password }
            };

            await Shell.Current.GoToAsync("ProfileSelectionView", parametros);
        }
        else
        {
            await MostrarErro(resposta.MensagemErro);
        }
    }

    private async Task MostrarErro(string mensagem)
    {
        await Application.Current.Windows[0].Page.DisplayAlertAsync("Atenção", mensagem, "OK");
    }

    [RelayCommand]
    private async Task GoBackToLoginAsync()
    {
        // Volta para trás na pilha de navegação
        await Shell.Current.GoToAsync("..");
    }
}