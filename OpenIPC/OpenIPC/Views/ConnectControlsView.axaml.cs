using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenIPC.ViewModels;

namespace OpenIPC.Views;

public partial class ConnectControlsView : UserControl
{
    public ConnectControlsView()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode)
        {
            DataContext = new ConnectControlsViewModel();
        }
    }
}