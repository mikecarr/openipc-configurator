using Avalonia.Controls;
using OpenIPC.ViewModels;

namespace OpenIPC.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode)
        {
            DataContext = new MainViewModel();
        }
    }
}