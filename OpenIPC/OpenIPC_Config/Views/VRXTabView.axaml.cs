using Avalonia.Controls;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class VRXTabView : UserControl
{
    public VRXTabView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode) DataContext = new VRXTabViewModel();
    }
}