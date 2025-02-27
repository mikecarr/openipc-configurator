using System.Threading.Tasks;

namespace OpenIPC_Config.Services;

public interface IGitHubService
{
    Task<string> GetGitHubDataAsync(string url);
}