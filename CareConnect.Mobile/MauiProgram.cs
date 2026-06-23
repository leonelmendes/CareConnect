using Microsoft.Extensions.Logging;
using DotNet.Meteor.HotReload.Plugin;

namespace CareConnect.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Placeholder", (h, v) =>
		{
		#if ANDROID
		h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
		#endif
		#if IOS
		h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
		#endif
		});

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
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

		return builder.Build();
	}
}
