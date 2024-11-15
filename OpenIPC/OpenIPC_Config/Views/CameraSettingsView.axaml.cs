using System.ComponentModel;
using Avalonia.Controls;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class CameraSettingsView : UserControl, INotifyPropertyChanged
{
    public CameraSettingsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode) DataContext = new CameraSettingsTabViewModel();
    }
}