using Serilog;

namespace OpenIPC_Config.Services;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class WifiConfigParser
{
    // Properties to store parsed values
    public int WifiChannel { get; private set; }
    public string WifiRegion { get; private set; }
    public string GsMavlinkPeer { get; private set; }
    public string GsVideoPeer { get; private set; }

    // Method to parse configuration from a string
    public void ParseConfigString(string configContent)
    {
        if (string.IsNullOrWhiteSpace(configContent))
        {
            Console.WriteLine("Config content is empty or null.");
            return;
        }

        var lines = configContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var configDict = new Dictionary<string, string>();

        // Regular expressions to match key-value pairs
        var keyValueRegex = new Regex(@"^\s*(\w+)\s*=\s*['""]?(.*?)['""]?\s*(?:#.*)?$");
        var sectionRegex = new Regex(@"^\s*\[.*\]\s*$");

        string currentSection = null;

        foreach (var line in lines)
        {
            // Skip empty lines and section headers
            if (string.IsNullOrWhiteSpace(line) || sectionRegex.IsMatch(line))
            {
                // Update current section (optional, for context tracking)
                var sectionMatch = sectionRegex.Match(line);
                if (sectionMatch.Success)
                {
                    currentSection = line.Trim();
                }
                continue;
            }

            // Match key-value pairs
            var match = keyValueRegex.Match(line);
            if (match.Success)
            {
                var key = match.Groups[1].Value.Trim();
                var value = match.Groups[2].Value.Trim();

                // Handle duplicate keys based on the current section context
                if (currentSection == "[gs_mavlink]" && key == "peer")
                {
                    GsMavlinkPeer = value;
                }
                else if (currentSection == "[gs_video]" && key == "peer")
                {
                    GsVideoPeer = value;
                }
                else
                {
                    // Store key-value pairs in a dictionary for general use
                    configDict[key] = value;
                }
            }
        }

        // Set variables based on parsed values
        if (configDict.TryGetValue("wifi_channel", out var wifiChannel))
        {
            WifiChannel = int.TryParse(wifiChannel, out var channel) ? channel : 0;
        }

        if (configDict.TryGetValue("wifi_region", out var wifiRegion))
        {
            WifiRegion = wifiRegion;
        }

        Log.Debug("Configuration Parsed Successfully:");
        Log.Debug($"Wifi Channel: {WifiChannel}");
        Log.Debug($"Wifi Region: {WifiRegion}");
        Log.Debug($"GS Mavlink Peer: {GsMavlinkPeer}");
        Log.Debug($"GS Video Peer: {GsVideoPeer}");
    }
}
