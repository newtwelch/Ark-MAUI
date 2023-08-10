using Microsoft.Extensions.Logging;
using Ark.Models;
using Ark.Models.Toast;
using Blazored.SessionStorage;
using Microsoft.Maui.LifecycleEvents;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Ark.Models.Songs;
#if WINDOWS
using System.Runtime.InteropServices;
    using Microsoft.UI;
    using Microsoft.UI.Windowing;
    using Windows.Graphics;
#endif

namespace Ark;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

#if WINDOWS
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        
        const UInt32 WM_CLOSE = 0x0010;

        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(lifeCycleBuilder =>
            {
                lifeCycleBuilder.OnWindowCreated(w =>
                {
                    w.ExtendsContentIntoTitleBar = false;
                    IntPtr wHandle = WinRT.Interop.WindowNative.GetWindowHandle(w);
                    WindowId windowId = Win32Interop.GetWindowIdFromWindow(wHandle);
                    AppWindow mauiWindow = AppWindow.GetFromWindowId(windowId);
                    mauiWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                    mauiWindow.Title = "Ark";
                });

                lifeCycleBuilder.OnClosed((w, args) => {
                     IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, "Display");
                     if (windowPtr == IntPtr.Zero)
                     {
                         Console.WriteLine("Window not found");
                         return;
                     }
                     
                     SendMessage(windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                } );
            });
        });

        
#endif
        builder
            .UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

        builder.Services.AddSingleton<ToastService>(); 
		builder.Services.AddSingleton<AppSharedState>();
		builder.Services.AddSingleton<DisplayService>();
		builder.Services.AddSingleton<SettingsService>();
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
