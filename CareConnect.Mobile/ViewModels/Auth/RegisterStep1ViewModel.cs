using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth;

public partial class RegisterStep1ViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nomeCompleto = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (Password != ConfirmPassword)
        {
            await App.Current.MainPage.DisplayAlert("Erro", "As palavras-passe não coincidem.", "OK");
            return;
        }

        // TODO: Validar o resto e registar no Firebase
        
        // Vai para a seleção de perfil
        await Shell.Current.GoToAsync("RegisterStep2View");
    }

    [RelayCommand]
    private async Task GoBackToLoginAsync()
    {
        // Volta para trás na pilha de navegação
        await Shell.Current.GoToAsync("..");
    }
}