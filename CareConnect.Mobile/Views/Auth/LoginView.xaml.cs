using CareConnect.Mobile.ViewModels.Auth;

namespace CareConnect.Mobile.Views.Auth;

public partial class LoginView : ContentPage
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    [Obsolete]
    public async void OnGoogleSignInClicked(object sender, EventArgs e)
    {
        // Lógica para iniciar o processo de login com o Google
        await DisplayAlert("Login", "Iniciando login com o Google...", "OK");
    }

    private async void OnShowPasswordTapped(object sender, EventArgs e)
    {
        // Alterna a visibilidade da senha
        passwordEntry.IsPassword = !passwordEntry.IsPassword;
        
    }
}