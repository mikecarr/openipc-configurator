using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace OpenIPC_Config.Services;

public class MessageBoxService : IMessageBoxService
{
    public async Task ShowMessageBox(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok);
        await msgBox.ShowAsync();
    }
}