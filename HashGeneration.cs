using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ModListHashChecker;

public class DictionaryHashGenerator
{
    public static string GenerateHash(Dictionary<string, BepInEx.PluginInfo> inputDictionary)
    {
        // Sort the values of the dictionary by key to ensure consistent order
        var sortedEntries = inputDictionary.OrderBy(entry => entry.Key);

        // Concatenate the sorted key-value pairs into a single string
        string concatenatedString = string.Join(",", sortedEntries.Select(entry => $"{entry.Key}:{entry.Value}"));

        // Convert the string to bytes
        byte[] inputBytes = Encoding.UTF8.GetBytes(concatenatedString);

        // Compute the hash
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(inputBytes);

        // Apply offset to each byte
        const int offset = 125;
        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashBytes[i] = (byte)(hashBytes[i] + offset);
        }

        // Convert the hash to a hexadecimal string
        StringBuilder stringBuilder = new();
        foreach (byte b in hashBytes)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }
}
