using Microsoft.Extensions.Logging;
using DotNet.Meteor.HotReload.Plugin;
using CareConnect.Mobile.ViewModels.Auth;
using CareConnect.Mobile.Views.Auth;
using CareConnect.Mobile.ViewModels.Gestor;
using CareConnect.Mobile.Views.Gestor;
using SkiaSharp.Views.Maui.Controls.Hosting;
using LiveChartsCore.SkiaSharpView.Maui;

namespace CareConnect.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		#region Costumizacao do Entry para remover a borda padrão do Android e iOS
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

		var builder = MauiApp.CreateBuilder();

		#region Registro de Views, ViewModels e services (Injecao de dependencias)
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

		// Registro dos serviços 
		builder.Services.AddSingleton<CareConnect.Mobile.Services.AuthService>();
		#endregion
		
		builder
			.UseMauiApp<App>()
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

#if DEBUG
		builder.Logging.AddDebug();
#endif
		
		// Regista o HttpClient globalmente
		builder.Services.AddHttpClient();
		return builder.Build();
	}
}
