#if WINDOWS
using Windows.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

using Ark.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Runtime.InteropServices;
using System.Text;

namespace Ark;

public partial class DisplayPage : ContentPage
{
    private SettingsService settingsService;

#if WINDOWS
	public DisplayPage(SettingsService _settingsService)
	{
		InitializeComponent();
        this.Loaded += (s, e) => { OnToggleFullscreenClicked(s, e); };
        settingsService = _settingsService;
    }

    private Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
        return appWindow;
    }

    private void OnToggleFullscreenClicked(object sender, EventArgs e)
    {
        var window = GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
        var appWindow = GetAppWindow(window);
        appWindow.Title = "Display";
        var presenter = appWindow.Presenter as OverlappedPresenter;
        
        presenter.SetBorderAndTitleBar(false, false);
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        presenter.IsResizable = false;
        
        var videosSelectors = FindWindowsWithText("Ark");

        foreach (IntPtr hwnd in videosSelectors)
        {
            Thread.Sleep(100);
            SetForegroundWindow(hwnd);
            break;
        }

        var monitors = Ark.Models.Monitor.All.ToArray();
        int selectedMonitorID = 0;
        var chosenMonitor = monitors[0];
        for(int i = 0; i < monitors.Length; i++){
            if(settingsService.ChosenMonitor == monitors[i].DeviceName)
            {
                chosenMonitor = monitors[i];
            }
        }

        var width = chosenMonitor.Bounds.Width;
        var height = chosenMonitor.Bounds.Height;
        appWindow.MoveAndResize(new RectInt32(chosenMonitor.WorkingArea.X, chosenMonitor.WorkingArea.Y, width, height));

        appWindow.SetPresenter(presenter);

    }



    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    // Delegate to filter which windows to include
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary> Get the text for the window pointed to by hWnd </summary>
    public static string GetWindowText(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd);
        if (size > 0)
        {
            var builder = new StringBuilder(size + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        return String.Empty;
    }

    /// <summary> Find all windows that match the given filter </summary>
    /// <param name="filter"> A delegate that returns true for windows
    ///    that should be returned and false for windows that should
    ///    not be returned </param>
    public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
    {
        IntPtr found = IntPtr.Zero;
        List<IntPtr> windows = new List<IntPtr>();

        EnumWindows(delegate (IntPtr wnd, IntPtr param)
        {
            if (filter(wnd, param))
            {
                // only add the windows that pass the filter
                windows.Add(wnd);
            }

            // but return true here so that we iterate all windows
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    /// <summary> Find all windows that contain the given title text </summary>
    /// <param name="titleText"> The text that the window title must contain. </param>
    public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
    {
        return FindWindows(delegate (IntPtr wnd, IntPtr param)
        {
            return GetWindowText(wnd).Contains(titleText);
        });
    }
#endif
}
