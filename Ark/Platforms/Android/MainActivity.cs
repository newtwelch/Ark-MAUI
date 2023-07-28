using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Ark.Models;

namespace Ark;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Platform.Init(this, savedInstanceState);

        this.Window?.AddFlags(WindowManagerFlags.Fullscreen);
    }

    public override bool DispatchKeyEvent(KeyEvent e)
    {

        if (e.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Down)
            {
                var service = MauiApplication.Current.Services.GetService<AppSharedState>();
                service.BackButtonPressed.Invoke();
            }
            return false;
        }
        return base.DispatchKeyEvent(e);
    }
}
