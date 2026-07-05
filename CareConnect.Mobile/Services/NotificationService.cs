using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;

namespace CareConnect.Mobile.Services;

public interface INotificationService
{
    Task MostrarSucessoAsync(string mensagem);
    Task MostrarErroAsync(string mensagem);
    Task MostrarAvisoAsync(string mensagem);
    Task MostrarToastAsync(string mensagem); 
}

public class NotificationService : INotificationService
{
    public async Task MostrarSucessoAsync(string mensagem)
    {
        await MostrarSnackbarAsync(mensagem, Colors.DarkGreen, "OK");
    }

    public async Task MostrarErroAsync(string mensagem)
    {
        await MostrarSnackbarAsync(mensagem, Colors.DarkRed, "FECHAR");
    }

    public async Task MostrarAvisoAsync(string mensagem)
    {
        await MostrarSnackbarAsync(mensagem, Colors.DarkOrange, "ENTENDI");
    }

    public async Task MostrarToastAsync(string mensagem)
    {
        var toast = Toast.Make(mensagem, ToastDuration.Short, 14);
        await toast.Show();
    }

    private async Task MostrarSnackbarAsync(string mensagem, Color corFundo, string textoBotao)
    {
        var opcoes = new SnackbarOptions
        {
            BackgroundColor = corFundo,
            TextColor = Colors.White,
            ActionButtonTextColor = Colors.White,
            CornerRadius = 10,
            Font = Font.OfSize("InterRegular", 14),
            ActionButtonFont = Font.OfSize("InterBold", 14)
        };

        var snackbar = Snackbar.Make(mensagem, null, textoBotao, TimeSpan.FromSeconds(4), opcoes);
        
        // Garante que é executado na thread principal da UI
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await snackbar.Show();
        });
    }
}