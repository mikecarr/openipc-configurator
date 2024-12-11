using System.Collections.Generic;

namespace OpenIPC_Config.Services;

public interface IYamlConfigService
{
    void ParseYaml(string content, Dictionary<string, string> yamlConfig);
    string UpdateYaml(Dictionary<string, string> yamlConfig);
}