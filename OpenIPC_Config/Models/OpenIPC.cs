using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

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


    public const string KeyMD5Sum = "24767056dc165963fe6db7794aee12cd";
//    public const string LocalFwFolder = "binaries/fw";


    public const string RemoteEtcFolder = "/etc";
    public const string RemoteBinariesFolder = "/usr/bin";
    public const string RemoteFontsFolder = "/usr/share/fonts/";
    public const string RemoteSensorsFolder = "/etc/sensors";
    public const string RemoteDroneKeyPath = "/etc/drone.key";
    public const string RemoteGsKeyPath = "/etc/gs.key";
    public const string RemoteTempFolder = "/tmp";

    public static string DroneKeyPath;
    public static string GsKeyPath;

    public static string LocalFirmwareFolder;
    public static string LocalBackUpFolder;
    
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

    public static string GetBinariesPath()
    {
        string basePath;

        // Determine the base path for the binaries folder
        if (OperatingSystem.IsMacOS())
        {
            // macOS: binaries will be inside the app bundle's resources folder
            basePath = AppContext.BaseDirectory; // Gets the app's execution directory
            return Path.Combine(basePath, "binaries");
        }
        else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            // Windows and Linux: binaries are alongside the app
            basePath = AppContext.BaseDirectory;
            return Path.Combine(basePath, "binaries");
        }
        
        else if (OperatingSystem.IsAndroid() )
        {
            basePath = AppContext.BaseDirectory;
            return basePath;
        }
        else if (OperatingSystem.IsIOS())
        {
            basePath = AppContext.BaseDirectory;
            return Path.Combine(basePath, "binaries");
        }

        throw new PlatformNotSupportedException("Unsupported platform");
    }
    
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
            LocalFirmwareFolder = Path.Combine(AppDataConfigDirectory, "firmware");
            
            DroneKeyPath =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "drone.key");
            GsKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "gs.key");
        }
        else if (OperatingSystem.IsIOS())
        {
            AppDataConfigDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);
            AppDataConfigPath = Path.Combine(AppDataConfigDirectory, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
            LocalFirmwareFolder = Path.Combine(AppDataConfigDirectory, "firmware");
            LocalBackUpFolder =  Path.Combine(AppDataConfigDirectory, "backup");
            
            DroneKeyPath =  Path.Combine(AppDataConfigDirectory, "binaries/drone.key");
            GsKeyPath = Path.Combine(AppDataConfigDirectory, "binaries/gs.key");
        }
        else if (OperatingSystem.IsWindows())
        {
            AppDataConfigDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            
            AppDataConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
            LocalFirmwareFolder = Path.Combine(AppDataConfigDirectory, "firmware");
            LocalBackUpFolder =  Path.Combine(AppDataConfigDirectory, "backup");
            
            DroneKeyPath =  Path.Combine(AppDataConfigDirectory, "binaries/drone.key");
            GsKeyPath = Path.Combine(AppDataConfigDirectory, "binaries/gs.key");
        }
        else if (OperatingSystem.IsMacOS())
        {
            AppDataConfigDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            AppDataConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
            LocalFirmwareFolder = Path.Combine(AppDataConfigDirectory, "firmware");
            LocalBackUpFolder =  Path.Combine(AppDataConfigDirectory, "backup");
            
            DroneKeyPath =  Path.Combine(AppDataConfigDirectory, "binaries/drone.key");
            GsKeyPath = Path.Combine(AppDataConfigDirectory, "binaries/gs.key");
        }
        else // Assume Linux
        {
            AppDataConfigDirectory = Path.Combine($"./config/{appName}");
            AppDataConfigPath = Path.Combine(AppDataConfigDirectory, "appsettings.json");
            DeviceSettingsConfigPath = Path.Combine(AppDataConfigDirectory, "openipc_config.json");
            LocalTempFolder = Path.Combine(AppDataConfigDirectory, "Temp");
            LocalFirmwareFolder = Path.Combine(AppDataConfigDirectory, "firmware");
            LocalBackUpFolder =  Path.Combine(AppDataConfigDirectory, "backup");
            
            DroneKeyPath =  Path.Combine(AppDataConfigDirectory, "binaries/drone.key");
            GsKeyPath = Path.Combine(AppDataConfigDirectory, "binaries/gs.key");
        }

        
            
        // Ensure the config directory exists
        Directory.CreateDirectory(AppDataConfigDirectory);
        Directory.CreateDirectory(LocalTempFolder);
        Directory.CreateDirectory(LocalFirmwareFolder);
        
    }
}