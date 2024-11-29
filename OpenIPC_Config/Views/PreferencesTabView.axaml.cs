
using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class PreferencesTabView : UserControl
{
    public PreferencesTabView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode) DataContext = new PreferencesTabViewModel();
        
        
    }

    
}