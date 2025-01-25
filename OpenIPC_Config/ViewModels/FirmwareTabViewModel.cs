using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using OpenIPC_Config.Services;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class FirmwareTabViewModel : ViewModelBase
{
    
    #region Observable Properties

    [ObservableProperty] private bool _canConnect;
    [ObservableProperty] private string _selectedFirmwareType;
    [ObservableProperty] private string _selectedManufacturer;
    [ObservableProperty] private string _selectedDeviceType;
    
    #endregion
    
    #region Collections

    [ObservableProperty] private ObservableCollection<string> _firmwareTypes;
    [ObservableProperty] private ObservableCollection<string> _manufacturers;
    [ObservableProperty] private ObservableCollection<string> _deviceTypes;
    #endregion
    
    public ICommand SelectFirmwareCommand { get; }
    
    
    private void InitializeCollections()
    {
        FirmwareTypes = new ObservableCollectionExtended<string>{"Ruby", "WFB"};
        Manufacturers = new ObservableCollectionExtended<string>{"OpenIPC", "Runcam", "Emax","Caddx"};
        DeviceTypes = new ObservableCollectionExtended<string>{"Mario", "Thinker", "Urllc","Caddx Fly", "EMax Wyvern Link","Runcam Wifilink"};
        
    }
    public FirmwareTabViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        InitializeCollections();
        
        SelectFirmwareCommand = new RelayCommand<Window>(async (window) => await SelectFirmware(window));
        
        
        
    }   
    
    public async Task SelectFirmware(Window window)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select a File",
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "Compressed", Extensions = { "tgz" } },
                new FileDialogFilter { Name = "Bin Files", Extensions = { "bin" } },
                new FileDialogFilter { Name = "All Files", Extensions = { "*" } }
            }
        };

        // Show the dialog and get the selected file(s)
        var result = await dialog.ShowAsync(window);
    
        if (result != null && result.Length > 0)
        {
            var selectedFile = result[0]; // Get the first selected file
            Console.WriteLine($"Selected File: {selectedFile}");
        }
    }
}