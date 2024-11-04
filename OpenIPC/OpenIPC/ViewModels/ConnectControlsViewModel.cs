using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC.Events;
using OpenIPC.Models;
using OpenIPC.Services;
using Prism.Events;
using ReactiveUI;
using Serilog;
using Color = System.Drawing.Color;

namespace OpenIPC.ViewModels;

public class ConnectControlsViewModel : ObservableObject
{
    private readonly Ping _ping = new Ping();
    
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    private readonly IEventAggregator _eventAggregator;

    private readonly DispatcherTimer _dispatcherTimer;
    private readonly SolidColorBrush _onlineColorBrush = new SolidColorBrush(Colors.Green);
    private readonly SolidColorBrush _offlineColorBrush = new SolidColorBrush(Colors.Red);
    public ICommand ConnectCommand { get; private set; }
    
    public ConnectControlsViewModel()
    {
        SetDefaults();
        _eventAggregator = App.EventAggregator;
        
        ConnectCommand = new RelayCommand(() => Connect());
        
        _cancellationTokenSource = new CancellationTokenSource();
        _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _dispatcherTimer.Tick += DispatcherTimer_Tick;
        _dispatcherTimer.Start();
    }
    
    private async void DispatcherTimer_Tick(object sender, EventArgs e)
    {
        await PingDeviceAsync();
    }

    private async Task PingDeviceAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(IpAddress);
                PingReply reply = await _ping.SendPingAsync(ipAddress, TimeSpan.FromSeconds(1), null, null, _cancellationTokenSource.Token);
                Log.Debug("Ping result: " + reply.Status.ToString());

                if (reply.Status == IPStatus.Success)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => PingStatusColor = _onlineColorBrush);
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() => PingStatusColor = _offlineColorBrush);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
    
    private void SetDefaults()
    {
        PingStatusColor = _offlineColorBrush;
        IpAddress = "192.168.1.10";
    }

    private string _ipAddress;
    public string? IpAddress
    {
        get => _ipAddress;
        set
        {
            SetProperty(ref _ipAddress, value);
            CheckIfCanConnect();    
        }
    }
    
    private string _password;
    public string? Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            CheckIfCanConnect();
        }
    }
    
    
    private bool _canConnect;
    

    public bool CanConnect
    {
        get => _canConnect;
        set => SetProperty(ref _canConnect, value);
    }

    private SolidColorBrush _pingStatusColor;
    public SolidColorBrush PingStatusColor
    {
        get { return _pingStatusColor; }
        set
        {
            _pingStatusColor = value;
            OnPropertyChanged(nameof(PingStatusColor));
        }
    }

    

    private void CheckIfCanConnect()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            CanConnect = !string.IsNullOrWhiteSpace(Password)
                         && !string.IsNullOrWhiteSpace(IpAddress);

            
            // CanConnect = !string.IsNullOrWhiteSpace(Password)
            //              && !string.IsNullOrWhiteSpace(IpAddress)
            //              && !SelectedDeviceType.Equals(DeviceType.None);
            //
            // Logger.Instance(_eventAggregator)
            //     .Log(
            //         $"********* CanConnect: {CanConnect}, Username: {Username}, Password: *****, IP: {IpAddress}, DeviceType: {SelectedDeviceType}()");
            //
            // _deviceConfig = new DeviceConfig();
            // _deviceConfig.Username = "root";
            // _deviceConfig.IpAddress = IpAddress;
            // _deviceConfig.Username = Username;
            // _deviceConfig.Password = Password;
            // _deviceConfig.DeviceType = DeviceType;
            //
            // // fire event
            // _eventAggregator?.GetEvent<DeviceStateUpdatedEvent>()
            //     .Publish(new DeviceStateUpdatedMessage(CanConnect, _deviceConfig));
        });
    }
    
    private void Connect()
    {
        DeviceConfig deviceConfig = new DeviceConfig();
        deviceConfig.IpAddress = IpAddress;
        deviceConfig.Password = Password;
        
        AppMessage appMessage = new AppMessage();
        appMessage.DeviceConfig = deviceConfig;
        appMessage.Message = "Hello from Connect ControlsViewModel";

        _eventAggregator.GetEvent<AppMessageEvent>().Publish(appMessage);

        SaveConfig(deviceConfig);
    }

    private void SaveConfig(DeviceConfig deviceConfig)
    {
        
        throw new NotImplementedException();
    }
}