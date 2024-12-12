using System.Threading.Tasks;

namespace OpenIPC_Config.Services;

public interface IMessageBoxService
{
    Task ShowMessageBox(string title, string message);
}