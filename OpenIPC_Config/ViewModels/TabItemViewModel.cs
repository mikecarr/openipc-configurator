namespace OpenIPC_Config.ViewModels;

public class TabItemViewModel
{
    public string TabName { get; }
    public object Content { get; }
    
    public string Icon { get; }
    
    public bool IsTabsCollapsed { get; set; }

    public TabItemViewModel(string tabName, string icon, object content, bool isTabsCollapsed )
    {
        TabName = tabName;
        Icon = icon;
        Content = content;
        IsTabsCollapsed = isTabsCollapsed;

    }
}