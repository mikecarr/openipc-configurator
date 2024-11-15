using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenIPC_Config.Services;

public static class Utilities
{
    public static string RemoveSpecialCharacters(string input)
    {
        var pattern = @"[^a-zA-Z0-9\-\@]"; // Matches any character that is not a letter, number, or dash
        var output = Regex.Replace(input, pattern, "");
        return output;
    }

    public static string ComputeSha256Hash(string rawData)
    {
        // Create a SHA256 instance
        using (var sha256Hash = SHA256.Create())
        {
            // Compute the hash as a byte array
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert the byte array to a hex string
            var builder = new StringBuilder();
            foreach (var b in bytes) builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }
}