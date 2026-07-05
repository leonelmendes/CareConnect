using System.Net;
using System.Net.Http.Headers;
using CareConnect.Mobile.Shells;

namespace CareConnect.Mobile.Services;

public class AuthInterceptor : DelegatingHandler
{
    private readonly AuthService _authService;
    private readonly INotificationService _notificationService;

    public AuthInterceptor(AuthService authService, INotificationService notificationService)
    {
        _authService = authService;
        _notificationService = notificationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Vai ao cofre buscar o token de 7 dias
        var token = await SecureStorage.Default.GetAsync("auth_token");

        // 2. Se o token existir, cola-o no cabeçalho HTTP da requisição
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 3. Envia o pedido para a API e aguarda a resposta
        var response = await base.SendAsync(request, cancellationToken);

        // 4. MAGIA: Se a API devolver 401 (Acesso Negado / Token Expirou)
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Limpa o token expirado (mas mantém o último e-mail salvo!)
            _authService.FazerLogout();

            // Mostra o Snackbar moderno em vez do alerta feio
            await _notificationService.MostrarAvisoAsync("A sua sessão de 7 dias expirou. Insira a palavra-passe para renovar.");

            // Redireciona para a tela inicial de Login na thread da UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current!.Windows[0].Page = new AppShell();
            });
        }

        return response;
    }
}