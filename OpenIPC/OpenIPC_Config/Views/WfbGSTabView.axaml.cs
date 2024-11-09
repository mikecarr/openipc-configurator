using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class WfbGSTabView : UserControl
{
    public WfbGSTabView()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode)
        {
            DataContext = new WfbGSTabViewModel();
        }
    }
}