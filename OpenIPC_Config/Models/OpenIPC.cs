using System;
using System.IO;
using System.Reflection;

namespace OpenIPC_Config.Models;

public class OpenIPC
{
    public enum FileType
    {
        Normal,
        BetaFlightFonts,
        iNavFonts,
        Sensors
    }

    public const string AppName = "OpenIPC_Config";

    public const string MajesticFileLoc = "/etc/majestic.yaml";
    public const string WfbConfFileLoc = "/etc/wfb.conf";
    public const string TelemetryConfFileLoc = "/etc/telemetry.conf";

    // Radxa files
    public const string WifiBroadcastFileLoc = "/etc/wifibroadcast.cfg";
    public const string WifiBroadcastModProbeFileLoc = "/etc/modprobe.d/wfb.conf";
    public const string ScreenModeFileLoc = "/config/scripts/screen-mode";


    public const string LocalBackUpFolder = "binaries";
    public const string LocalBinariesFolder = "binaries";
    public const string LocalSensorsFolder = "binaries/sensors";
    public const string LocalFontsFolder = "binaries/fonts";
    public const string LocalBetaFlightFontsFolder = "binaries/fonts/bf";
    public const string LocalINavFontsFolder = "binaries/fonts/inav";

//    public const string LocalFwFolder = "binaries/fw";


    public const string RemoteEtcFolder = "/etc";
    public const string RemoteBinariesFolder = "/usr/bin";
    public const string RemoteFontsFolder = "/usr/share/fonts/";
    public const string RemoteSensorsFolder = "/etc/sensors";
    public const string RemoteDroneKeyPath = "/etc/drone.key";
    public const string RemoteGsKeyPath = "/etc/gs.key";
    public const string RemoteTempFolder = "/tmp";

    public const string DroneKeyPath = "binaries/drone.key";

    // Base application data folder path for your app
    // public static readonly string LocalAppDataFolder = Path.Combine(
    //     Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);


    public static readonly string LocalFirmwareFolder = $"{LocalTempFolder}/firmware";

    public static string DeviceUsername = "root";

    static OpenIPC()
    {
        InitializePaths();
    }

    // Expose configPath and configDirectory as public static properties
    public static string AppDataConfigDirectory { get; private set; }

    public static string LocalTempFolder { get; private set; }
    public static string AppDataConfigPath { get; private set; }

    public static string DeviceSettingsConfigPath { get; private set; }

    private static void InitializePaths()
    {
        var appName = Assembly.GetExecutingAssembly().GetName().Name;

        if (OperatingSystem.IsAndroid())
        {
            // Android-specific path
            AppDataConfigDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            AppDataConfigPath = Path.Combine(AppDataConfigDirectory, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
        }
        else if (OperatingSystem.IsIOS())
        {
            AppDataConfigDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);
            AppDataConfigPath = Path.Combine(AppDataConfigDirectory, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
        }
        else if (OperatingSystem.IsWindows())
        {
            AppDataConfigDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            AppDataConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
        }
        else if (OperatingSystem.IsMacOS())
        {
            AppDataConfigDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            AppDataConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
        }
        else // Assume Linux
        {
            AppDataConfigDirectory = Path.Combine($"./config/{appName}");
            AppDataConfigPath = Path.Combine(AppDataConfigDirectory, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
        }

        // Ensure the config directory exists
        Directory.CreateDirectory(AppDataConfigDirectory);
        Directory.CreateDirectory(LocalTempFolder);
    }
}