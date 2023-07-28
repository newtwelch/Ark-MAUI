using Microsoft.Extensions.Logging;
using Ark.Models;
using Ark.Models.Toast;
using Blazored.SessionStorage;
using Microsoft.Maui.LifecycleEvents;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Ark;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

        builder.Services.AddSingleton<ToastService>(); 
		builder.Services.AddSingleton<AppSharedState>();
		builder.Services.AddSingleton<DisplayService>();
		builder.Services.AddBlazoredSessionStorage();
        builder.Services.AddMauiBlazorWebView(); 
		builder.Services.AddHotKeys2();
        builder.Services.AddSingleton<SongService>(); 
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
        builder.Services.AddBlazorWebViewDeveloperTools();
        return builder.Build();
	}
}
