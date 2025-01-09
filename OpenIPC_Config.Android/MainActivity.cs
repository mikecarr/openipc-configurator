using System;
using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using OpenIPC_Config.Android.Helpers;

namespace OpenIPC_Config.Android;

[Activity(
    Label = "OpenIPC_Config.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        
        AndroidFileHelper.CopyAssetsToInternalStorage(global::Android.App.Application.Context);      
        
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}