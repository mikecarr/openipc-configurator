using System.ComponentModel;
using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class CameraSettingsView : UserControl, INotifyPropertyChanged
{
    public CameraSettingsView()
    {
        InitializeComponent();
        //if (!Design.IsDesignMode) DataContext = new CameraSettingsTabViewModel();
        DataContext = App.ServiceProvider.GetService<CameraSettingsTabViewModel>();
    }
}