using CareConnect.Mobile.Services;
using CareConnect.Mobile.Shells;
using CareConnect.Mobile.ViewModels;
using CareConnect.Mobile.ViewModels.Auth;
using CareConnect.Mobile.ViewModels.Cuidador;
using CareConnect.Mobile.ViewModels.Gestor;
using CareConnect.Mobile.ViewModels.Shared;
using CareConnect.Mobile.Views.Auth;
using CareConnect.Mobile.Views.Cuidador;
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

        #region CUSTOMIZAÇÃO GLOBAL DE CONTROLOS (BORDERLESS)

        // 1. CUSTOMIZAÇÃO DO ENTRY (Campos de Texto Simples)
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEntry", (handler, view) =>
        {
        #if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        #elif IOS
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
        #endif
        });

        // 2. CUSTOMIZAÇÃO DO EDITOR (Campos de Texto Multilinha)
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEditor", (handler, view) =>
        {
        #if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        #elif IOS
            // No iOS o Editor usa UITextView que não tem BorderStyle, removemos o fundo nativo se necessário
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
        #endif
        });

        // 3. CUSTOMIZAÇÃO DO PICKER (Seleções de Lista)
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("BorderlessPicker", (handler, view) =>
        {
        #if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        #elif IOS
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
        #endif
        });

        // 4. CUSTOMIZAÇÃO DO DATEPICKER (Seleção de Datas)
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("BorderlessDatePicker", (handler, view) =>
        {
        #if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        #elif IOS
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
        #endif
        });

        // 5. CUSTOMIZAÇÃO DO TIMEPICKER (Seleção de Horas)
        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("BorderlessTimePicker", (handler, view) =>
        {
        #if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        #elif IOS
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
        #endif
        });

        #endregion

        #region REGISTO DE SERVIÇOS E SHELLS
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddTransient<AuthInterceptor>();
        builder.Services.AddScoped<PatientService>();
        builder.Services.AddScoped<CarePlanService>();
        builder.Services.AddSingleton<TarefaService>();

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

        builder.Services.AddTransient<GestorPlanosViewModel>();
        builder.Services.AddTransient<GestorPlanosView>();

        builder.Services.AddTransient<DetalhePlanoViewModel>();
        builder.Services.AddTransient<DetalhePlanoView>();

        builder.Services.AddTransient<CriarPlanoCuidadoViewModel>();
        builder.Services.AddTransient<CriarPlanoCuidadoView>();

        builder.Services.AddTransient<EditarUtenteViewModel>();
        builder.Services.AddTransient<EditarUtenteView>();

        builder.Services.AddTransient<CuidadorHomeViewModel>();
        builder.Services.AddTransient<CuidadorHomeView>();

        builder.Services.AddTransient<RegistoAdHocView>();
        builder.Services.AddTransient<RegistoAdHocViewModel>();

        #endregion

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        return builder.Build();
    }
}