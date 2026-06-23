namespace CareConnect.Mobile.Views;

public partial class LoginView : ContentPage
{
    public LoginView()
    {
        InitializeComponent();
    }

    [Obsolete]
    public async void OnGoogleSignInClicked(object sender, EventArgs e)
    {
        // Lógica para iniciar o processo de login com o Google
        await DisplayAlert("Login", "Iniciando login com o Google...", "OK");
    }
}