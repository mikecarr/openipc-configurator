namespace OpenIPC_Config.ViewModels;

public class TabItemViewModel
{
    public TabItemViewModel(string tabName, string icon, object content, bool isTabsCollapsed)
    {
        TabName = tabName;
        Icon = icon;
        Content = content;
        IsTabsCollapsed = isTabsCollapsed;
    }

    public string TabName { get; }
    public object Content { get; }

    public string Icon { get; }

    public bool IsTabsCollapsed { get; set; }
}