using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenIPC_Config.Services;

public static class Utilities
{
    public static string RemoveSpecialCharacters(string input)
    {
        string pattern = @"[^a-zA-Z0-9\-\@]"; // Matches any character that is not a letter, number, or dash
        string output = Regex.Replace(input, pattern, "");
        return output;
    }
    
    public static string ComputeSha256Hash(string rawData)
    {
        // Create a SHA256 instance
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Compute the hash as a byte array
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert the byte array to a hex string
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}