using System.ComponentModel;
using System.Runtime.CompilerServices;
using Prism.Events;

namespace OpenIPC_Config.Models;

/// <summary>
/// Represents the configuration for a connected device
/// </summary>
public class DeviceConfig : INotifyPropertyChanged
{
    #region Private Fields
    private static DeviceConfig _instance;
    private string _hostname;
    private string _ipAddress;
    private string _keyChksum;
    private string _password;
    private string _chipType;
    private int _port;
    private string _username;
    #endregion

    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of DeviceConfig
    /// </summary>
    public DeviceConfig()
    {
        Username = "root";
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets or sets the event aggregator for device events
    /// </summary>
    public IEventAggregator EventAggregator { get; set; }

    /// <summary>
    /// Gets the singleton instance of DeviceConfig
    /// </summary>
    public static DeviceConfig Instance => _instance ??= new DeviceConfig();

    /// <summary>
    /// Gets or sets the username for device authentication
    /// </summary>
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the password for device authentication
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the IP address of the device
    /// </summary>
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the hostname of the device
    /// </summary>
    public string Hostname
    {
        get => _hostname;
        set
        {
            _hostname = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the port number for device communication
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the key checksum for device validation
    /// </summary>
    public string KeyChksum
    {
        get => _keyChksum;
        set
        {
            _keyChksum = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the type of the device
    /// </summary>
    public DeviceType DeviceType { get; set; }

    
    /// <summary>
    /// Gets or sets the chip type of the device
    /// </summary>
    public string ChipType
    {
        get => _chipType;
        set
        {
            _chipType = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Gets whether the device can be connected to based on required properties
    /// </summary>
    public bool CanConnect =>
        !string.IsNullOrEmpty(Hostname) &&
        !string.IsNullOrEmpty(IpAddress) &&
        !string.IsNullOrEmpty(Password);
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the singleton instance of DeviceConfig
    /// </summary>
    public static void SetInstance(DeviceConfig instance)
    {
        _instance = instance;
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Raises the PropertyChanged event
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}