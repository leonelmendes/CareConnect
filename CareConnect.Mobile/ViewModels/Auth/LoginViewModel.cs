using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth
{
    public partial class LoginViewModel : ObservableObject
    {
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await App.Current.MainPage.DisplayAlert("Erro", "Preencha todos os campos.", "OK");
            return;
        }

        // TODO: Chamar o Firebase Auth aqui no futuro
        
        // Simulação de Sucesso -> Vai para o ecrã de seleção de perfil
        await Shell.Current.GoToAsync("RegisterStep2View"); 
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("RegisterStep1View");
    }

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync("ForgotPasswordView");
    }
}