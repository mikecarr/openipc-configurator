namespace OpenIPC_Config.Services;

using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class WfbGsConfigParser
{
    // Properties to store parsed values
    public string Frequency { get; set; }
    public string TxPower { get; set; }
    public string McsIndex { get; set; }
    public string Stbc { get; set; }
    public string Unit { get; set; }
    public string Wlan { get; set; }
    public string Region { get; set; }

    private readonly Dictionary<string, string> configDict = new();

    // Method to parse configuration from a string
    public void ParseConfigString(string configContent)
    {
        if (string.IsNullOrWhiteSpace(configContent))
        {
            Log.Error("Config content is empty or null.");
            return;
        }

        var lines = configContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // Regular expressions to match key-value pairs and options line
        var keyValueRegex = new Regex(@"^\s*(\w+)\s*=\s*['""]?(.*?)['""]?\s*(?:#.*)?$");
        var optionsRegex = new Regex(@"^\s*options\s+88XXau_wfb\s+(.+)$");

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            // Match key-value pairs
            // var match = keyValueRegex.Match(trimmedLine);
            // if (match.Success)
            // {
            //     var key = match.Groups[1].Value.Trim();
            //     var value = match.Groups[2].Value.Trim();
            //     configDict[key] = value;
            //
            //     // Update properties based on key
            //     switch (key)
            //     {
            //         case "frequency":
            //             Frequency = value;
            //             break;
            //         case "txpower":
            //             TxPower = value;
            //             break;
            //         case "mcs_index":
            //             McsIndex = value;
            //             break;
            //         case "stbc":
            //             Stbc = value;
            //             break;
            //         case "unit":
            //             Unit = value;
            //             break;
            //         case "wlan":
            //             Wlan = value;
            //             break;
            //         case "region":
            //             Region = value;
            //             break;
            //     }
            // }

            // Check for options line (e.g., "options 88XXau_wfb rtw_tx_pwr_idx_override=1")
            var optionsMatch = optionsRegex.Match(trimmedLine);
            if (optionsMatch.Success)
            {
                var val = optionsMatch.Groups[1].Value; 
                // rtw_tx_pwr_idx_override=1
                TxPower = optionsMatch.Groups[1].Value;
                var index = val.IndexOf('=');
                if (index != -1)
                {
                    TxPower = val.Substring(index + 1).Trim(); // Extracts "1"
                }
            }
        }

        Log.Debug("WFB Configuration Parsed Successfully:");
        Log.Debug($"Frequency: {Frequency}");
        Log.Debug($"TxPower: {TxPower}");
        Log.Debug($"MCS Index: {McsIndex}");
        Log.Debug($"STBC: {Stbc}");
        Log.Debug($"Unit: {Unit}");
        Log.Debug($"Wlan: {Wlan}");
        Log.Debug($"Region: {Region}");
    }

    // Method to update the configuration dictionary with the current properties
    private void UpdateConfigDict()
    {
        if (!string.IsNullOrEmpty(Frequency)) configDict["frequency"] = Frequency;
        if (!string.IsNullOrEmpty(TxPower)) configDict["txpower"] = TxPower;
        if (!string.IsNullOrEmpty(McsIndex)) configDict["mcs_index"] = McsIndex;
        if (!string.IsNullOrEmpty(Stbc)) configDict["stbc"] = Stbc;
        if (!string.IsNullOrEmpty(Unit)) configDict["unit"] = Unit;
        if (!string.IsNullOrEmpty(Wlan)) configDict["wlan"] = Wlan;
        if (!string.IsNullOrEmpty(Region)) configDict["region"] = Region;
    }

    // Method to generate the updated configuration string
    public string GetUpdatedConfigString()
    {
        UpdateConfigDict();

        var sb = new StringBuilder();

        foreach (var entry in configDict)
        {
            sb.AppendLine($"{entry.Key}={entry.Value}");
        }

        // Add the options line if TxPower was updated via options line
        if (!string.IsNullOrEmpty(TxPower))
        {
            sb.AppendLine($"options 88XXau_wfb rtw_tx_pwr_idx_override={TxPower}");
        }

        return sb.ToString();
    }
}
