using CareConnect.Mobile.Services;
using CareConnect.Mobile.Shells;
using CareConnect.Mobile.ViewModels.Auth;
using CareConnect.Mobile.ViewModels.Gestor;
using CareConnect.Mobile.ViewModels.Shared;
using CareConnect.Mobile.Views.Auth;
using CareConnect.Mobile.Views.Gestor;
using CareConnect.Mobile.Views.Shared;
using CommunityToolkit.Maui;
using DotNet.Meteor.HotReload.Plugin;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace CareConnect.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .UseLiveCharts()
#if DEBUG
            .EnableHotReload()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Inter_18pt-Bold.ttf", "InterBold");
                fonts.AddFont("Inter_18pt-Semibold.ttf", "InterSemiBold");
                fonts.AddFont("Inter_18pt-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter_18pt-Medium.ttf", "InterMedium");
            });

        #region CUSTOMIZAÇÃO DO ENTRY
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Placeholder", (h, v) =>
        {
#if ANDROID
            h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
#if IOS
            h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });
		#endregion

        #region REGISTO DE SERVIÇOS E SHELLS
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddTransient<AuthInterceptor>(); // CRÍTICO: O intercetor tem de ser registado aqui!

        // 4. CONFIGURAÇÃO DE HTTP CLIENTS (Sem Dependências Circulares!)
        // A) Cliente do AuthService (Usado para Login/Registo -> NÃO LEVA INTERCETOR)
        builder.Services.AddHttpClient<AuthService>(client =>
        {
            client.BaseAddress = new Uri(Constants.BaseUrl);
        });

        // B) Cliente Global "CareConnectAPI" (Usado para Utentes/Tarefas -> LEVA O INTERCETOR)
        builder.Services.AddHttpClient("CareConnectAPI", client =>
        {
            client.BaseAddress = new Uri(Constants.BaseUrl);
        })
        .AddHttpMessageHandler<AuthInterceptor>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<GestorShell>();
        builder.Services.AddSingleton<CuidadorShell>();
        #endregion

        #region REGISTO DE VIEWS E VIEWMODELS
        builder.Services.AddTransient<ProfileSelectionViewModel>(); 
        builder.Services.AddTransient<ProfileSelectionView>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginView>();

        builder.Services.AddTransient<RegisterStep1ViewModel>();
        builder.Services.AddTransient<RegisterStep1View>();

        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<OnboardingView>();

        builder.Services.AddTransient<GestorHomeViewModel>();
        builder.Services.AddTransient<GestorHomeView>();

        builder.Services.AddTransient<UtentesViewModel>();
        builder.Services.AddTransient<UtentesView>();

        builder.Services.AddTransient<DetalheUtenteViewModel>();
        builder.Services.AddTransient<DetalheUtenteView>();

        builder.Services.AddTransient<PerfilViewModel>();
        builder.Services.AddTransient<PerfilView>();

        builder.Services.AddTransient<AdicionarUtenteViewModel>();
        builder.Services.AddTransient<AdicionarUtenteView>();

        #endregion

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        return builder.Build();
    }
}