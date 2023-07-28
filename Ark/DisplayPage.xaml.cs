#if WINDOWS
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace Ark;

public partial class DisplayPage : ContentPage
{
	public DisplayPage()
	{
		InitializeComponent();
#if WINDOWS
        this.Loaded += (s, e) => { OnToggleFullscreenClicked(s, e); };
        
#endif
    }

#if WINDOWS
    private Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindow;
    }
#endif

    private void OnToggleFullscreenClicked(object sender, EventArgs e)
    {
#if WINDOWS
        var window = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        var appWindow = GetAppWindow(window);
        var monitors = Ark.Models.Monitor.All.ToArray();
        if (monitors.Length > 1)
        {
            var thisMonitor = Ark.Models.Monitor.FromWindow(WinRT.Interop.WindowNative.GetWindowHandle(window));
            var otherMonitor = monitors[1];
            // move to second display's upper left corner
            appWindow.Move(new PointInt32(otherMonitor.WorkingArea.X, otherMonitor.WorkingArea.Y));
        }

        appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
#endif
    }
}
