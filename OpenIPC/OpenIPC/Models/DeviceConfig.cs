namespace OpenIPC.Models;

public class DeviceConfig
{
    public DeviceConfig()
    {
        Username = "root";
    }
    
    public string Username { get; set; }
    public string Password { get; set; }
    public string IpAddress { get; set; }  
    public string Hostname { get; set; }
    public DeviceType DeviceType { get; set; }
    
    public override string ToString()
    {
        return $"DeviceConfig{{Hostname={Hostname}, Username={Username}, Password=*****, IpAddress={IpAddress}, DeviceType={DeviceType}}}";
    }
    
}

public enum DeviceType
{
    None,
    Camera,
    Radxa,
    NVR
}