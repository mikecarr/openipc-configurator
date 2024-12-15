using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json.Linq;
using OpenIPC_Config.Logging;
using OpenIPC_Config.Services;
using OpenIPC_Config.ViewModels;
using OpenIPC_Config.Views;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config;

public class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure and build the DI container
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        CreateAppSettings();
        
        // check for updates
        CheckForUpdatesAsync();


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Remove Avalonia's default data validation plugin to avoid conflicts
            BindingPlugins.DataValidators.RemoveAt(0);

            // Resolve MainWindow and its DataContext from DI container
            desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Resolve MainView and its DataContext from DI container
            singleViewPlatform.MainView = ServiceProvider.GetRequiredService<MainView>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public virtual async Task ShowUpdateDialogAsync(string releaseNotes, string downloadUrl)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard("Update Available",
            $"New version available:\n\n{releaseNotes}\n\nDo you want to download the update?", ButtonEnum.YesNo);

        var result = await msgBox.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            OpenBrowser(downloadUrl);
        }
    }

    private void OpenBrowser(string url)
    {
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    
    private async Task CheckForUpdatesAsync()
    {
        // Set up the necessary dependencies
        var httpClient = new HttpClient();

        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Assembly.GetExecutingAssembly().GetName().Name, "appsettings.json");
        
        // Create an IConfiguration instance
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();

        // Pass the dependencies to the constructor
        var updateChecker = new UpdateChecker(httpClient, configuration);

        try
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var result = await updateChecker.CheckForUpdateAsync(currentVersion);

            if (result.HasUpdate)
            {
                await ShowUpdateDialogAsync(result.ReleaseNotes, result.DownloadUrl);
                Log.Information($"Update Available! Version: {result.ReleaseNotes}");
            }
            else
            {
                Log.Information("No updates found.");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking for updates: {ex.Message}");
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register IEventAggregator as a singleton
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IEventSubscriptionService, EventSubscriptionService>();
        services.AddSingleton<ISshClientService, SshClientService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        
        services.AddSingleton<IYamlConfigService, YamlConfigService>();
        services.AddSingleton<ILogger>(sp => Log.Logger);


        // Register ViewModels
        services.AddTransient<MainViewModel>();

        services.AddTransient<CameraSettingsTabViewModel>();
        services.AddTransient<ConnectControlsViewModel>();
        services.AddTransient<LogViewerViewModel>();
        services.AddTransient<SetupTabViewModel>();
        services.AddTransient<StatusBarViewModel>();
        services.AddTransient<TelemetryTabViewModel>();
        services.AddTransient<VRXTabViewModel>();
        services.AddTransient<WfbGSTabViewModel>();
        services.AddTransient<WfbTabViewModel>();        

        // Register Views
        services.AddTransient<MainWindow>();
        services.AddTransient<MainView>();
        services.AddTransient<CameraSettingsView>();
        services.AddTransient<ConnectControlsView>();
        services.AddTransient<LogViewer>();
        services.AddTransient<SetupTabView>();
        services.AddTransient<StatusBarView>();
        services.AddTransient<TelemetryTabView>();
        services.AddTransient<VRXTabView>();
        services.AddTransient<WfbGSTabView>();
        services.AddTransient<WfbTabView>();

    }
    

    private void CreateAppSettings()
    {
        string configPath;
        string configDirectory;

        var appName = Assembly.GetExecutingAssembly().GetName().Name;
        Log.Information($"Application name: {appName}, running on {RuntimeInformation.OSDescription}");
        if (OperatingSystem.IsAndroid())
        {
            // Android-specific path
            configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName, "appsettings.json");
        }
        else if (OperatingSystem.IsIOS())
        {
            configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);
            configPath = Path.Combine(configDirectory, "appsettings.json");
        }
        else if (OperatingSystem.IsWindows())
        {
            configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "appsettings.json");
        }
        else if (OperatingSystem.IsMacOS())
        {
            configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "appsettings.json");
        }
        else // Assume Linux
        {
            configDirectory = Path.Combine($"./config/{appName}");
            configPath = Path.Combine($"./config/{appName}", "appsettings.json");
        }

        if (!Directory.Exists(configDirectory))
            Directory.CreateDirectory(configDirectory);

        // Check if appsettings.json exists, otherwise create a default one
        if (!File.Exists(configPath))
        {
            // Create default settings
            var defaultSettings = createDefaultAppSettings();

            File.WriteAllText(configPath, defaultSettings.ToString());

            Thread.Sleep(2000);

            Log.Information($"Default appsettings.json created at {configPath}");
        }


        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, false, true)
            .AddJsonFile("appsettings.json", true, true)
            // .AddJsonFile("appsettings.Development.json", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            // .AddJsonFile(
            //     $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
            //     true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sink(new EventAggregatorSink(ServiceProvider.GetRequiredService<IEventAggregator>()))
            .CreateLogger();

        Log.Information(
            "**********************************************************************************************");
        Log.Information($"Starting up log for OpenIPC Configurator v{VersionHelper.GetAppVersion()}");
        Log.Information($"Using appsettings.json from {configPath}");
        
    }

    private JObject createDefaultAppSettings()
    {
        // Create default settings
        var defaultSettings = new JObject(
            new JProperty("UpdateChecker",
                new JObject(
                    new JProperty("LatestJsonUrl", "https://github.com/OpenIPC/openipc-configurator/releases/latest/download/latest.json")
                )
                    
            ),
            new JProperty("Serilog",
                new JObject(
                    new JProperty("Using", new JArray("Serilog.Sinks.Console", "Serilog.Sinks.RollingFile")),
                    new JProperty("MinimumLevel", "Debug"),
                    new JProperty("WriteTo",
                        new JArray(
                            new JObject(
                                new JProperty("Name", "Console")
                            ),
                            new JObject(
                                new JProperty("Name", "RollingFile"),
                                new JProperty("Args",
                                    new JObject(
                                        new JProperty("pathFormat",
                                            $"{Models.OpenIPC.AppDataConfigDirectory}/Logs/configurator-{{Date}}.log") 
                                        
                                    )
                                )
                            )
                        )
                    ),
                    new JProperty("Properties",
                        new JObject(
                            new JProperty("Application", "OpenIPC_Config")
                        )
                    )
                )
            )
        );
        return defaultSettings;
    }
}
