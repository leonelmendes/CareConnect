using CareConnect.Mobile.Services;
using CareConnect.Mobile.Shells;
using CareConnect.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CareConnect.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Aplica o tema salvo assim que a aplicação arranca:
		bool isDark = Preferences.Default.Get("app_theme_dark", false);
		Application.Current!.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var dataExpiracaoStr = Preferences.Default.Get("auth_expiration", string.Empty);
		var perfil = Preferences.Default.Get("auth_profile", "Gestor");
		bool temSessaoValida = false;

		if (DateTime.TryParse(dataExpiracaoStr, out DateTime dataExpiracao))
		{
			// Se a data de expiração for maior que o momento atual, o token ainda serve!
			if (dataExpiracao > DateTime.UtcNow)
			{
				temSessaoValida = true;
			}
		}

		Page? paginaInicial;

		if (temSessaoValida)
		{
			// Pula o login e vai direto para a Dashboard do seu perfil
			paginaInicial = perfil == "Gestor" ? new GestorShell() : new CuidadorShell();

			// Dispara a renovação silenciosa em background (sem travar a interface)
			Task.Run(async () => await RenovarTokenEmBackgroundAsync(activationState));
		}
		else
		{
			// Token expirou ou é a primeira vez na app -> Vai para o Login
			paginaInicial = activationState?.Context.Services.GetRequiredService<AppShell>();
		}

		return new Window(paginaInicial!);
	}

	// Método privado auxiliar para renovar o token sem o utilizador notar
	private async Task RenovarTokenEmBackgroundAsync(IActivationState? activationState)
	{
		try
		{
			var authService = activationState?.Context.Services.GetService<AuthService>();
			if (authService != null)
			{
				await authService.RenovarTokenSilenciosoAsync();
			}
		}
		catch 
		{ 
			// Se falhar (ex: sem internet), não faz mal. 
			// O token atual ainda é válido e o Intercetor tratará de expulsar o utilizador 
			// apenas no dia em que o token antigo expirar de vez.
		}
	}
}