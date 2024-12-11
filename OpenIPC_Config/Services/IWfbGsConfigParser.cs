namespace OpenIPC_Config.Services;

public interface IWfbGsConfigParser
{
    string TxPower { get; set; }
    string GetUpdatedConfigString();
}