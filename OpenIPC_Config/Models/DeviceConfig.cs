using System.ComponentModel;
using System.Runtime.CompilerServices;
using Prism.Events;

namespace OpenIPC_Config.Models;

public class DeviceConfig : INotifyPropertyChanged
{
    private static DeviceConfig _instance;
    private string _hostname;
    private string _ipAddress;
    private string _keyChksum;
    private string _password;
    private int _port;

    private string _username;

    public DeviceConfig()
    {
        Username = "root";
    }

    public IEventAggregator EventAggregator { get; set; }
    public static DeviceConfig Instance => _instance ??= new DeviceConfig();

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    public string Hostname
    {
        get => _hostname;
        set
        {
            _hostname = value;
            OnPropertyChanged();
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            OnPropertyChanged();
        }
    }

    public string KeyChksum
    {
        get => _keyChksum;
        set
        {
            _keyChksum = value;
            OnPropertyChanged();
        }
    }

    public DeviceType DeviceType { get; set; }

    // CanConnect property to determine connection eligibility
    public bool CanConnect =>
        !string.IsNullOrEmpty(Hostname) &&
        !string.IsNullOrEmpty(IpAddress) &&
        !string.IsNullOrEmpty(Password);

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}