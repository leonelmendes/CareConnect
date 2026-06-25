namespace CareConnect.Mobile.Views.Auth;

public partial class ForgotPasswordView : ContentPage
{
    public ForgotPasswordView()
    {
        InitializeComponent();
    }

    private async void OnSendLinkTapped(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Aviso", "Por favor, insira o seu endereço de email.", "OK");
            return;
        }

        // TODO: Futuramente, aqui ficará a chamada direta ao Firebase:
        // await FirebaseAuth.Instance.SendPasswordResetEmailAsync(email);

        // Feedback visual para o utilizador
        await DisplayAlert("Verifique o seu email!", 
                           $"Enviámos um link mágico de recuperação para {email}. Siga as instruções para criar uma nova palavra-passe.", 
                           "Entendido");

        // Após enviar o email, voltamos ao ecrã de Login para não o deixar num beco sem saída
        await Shell.Current.GoToAsync("..");
    }

    private async void OnBackToLoginTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}