using System.ComponentModel;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class CameraSettingsTabView : UserControl, INotifyPropertyChanged
{
    public CameraSettingsTabView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode) DataContext = App.ServiceProvider.GetService<CameraSettingsTabViewModel>();
    }
}