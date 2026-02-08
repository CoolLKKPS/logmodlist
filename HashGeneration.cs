using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ModListHashChecker;

public class DictionaryHashGenerator
{
    public static string GenerateModListString(Dictionary<string, BepInEx.PluginInfo> inputDictionary)
    {
        // Sort the values of the dictionary by key to ensure consistent order
        var sortedEntries = inputDictionary.OrderBy(entry => entry.Key);
        return string.Join(",", sortedEntries.Select(entry => $"{entry.Key}:{entry.Value}"));
    }

    public static string GenerateHash(Dictionary<string, BepInEx.PluginInfo> inputDictionary)
    {
        // Concatenate the sorted key-value pairs into a single string
        string concatenatedString = GetFullModListString(inputDictionary);

        // Append salt
        concatenatedString += "TeamMLC";

        // Convert the string to bytes
        byte[] inputBytes = Encoding.UTF8.GetBytes(concatenatedString);

        // Compute the hash
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(inputBytes);
        StringBuilder stringBuilder = new();
        foreach (byte b in hashBytes)
        {
            stringBuilder.Append(b.ToString("x2"));
        }
        return stringBuilder.ToString();
    }

    public static string ComputeHash(string input, string salt)
    {
        string combined = input + salt;
        byte[] inputBytes = Encoding.UTF8.GetBytes(combined);
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(inputBytes);

        // Convert the hash to a hexadecimal string
        StringBuilder stringBuilder = new();
        foreach (byte b in hashBytes)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    public static string ComputeFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        byte[] hashBytes = sha256.ComputeHash(stream);
        StringBuilder stringBuilder = new();
        foreach (byte b in hashBytes)
        {
            stringBuilder.Append(b.ToString("x2"));
        }
        return stringBuilder.ToString();
    }

    public static string GetFullModListString(Dictionary<string, BepInEx.PluginInfo> pluginInfos)
    {
        var components = new List<string>();

        string pluginString = GenerateModListString(pluginInfos);
        components.Add($"plugins:{pluginString}");

        string root = BepInEx.Paths.BepInExRootPath;
        string patchersPath = Path.Combine(root, "patchers");
        if (Directory.Exists(patchersPath))
        {
            var patcherFiles = Directory.EnumerateFiles(patchersPath, "*.dll", SearchOption.AllDirectories)
                .Select(f => new { FullPath = f, RelativePath = GetRelativePath(root, f) })
                .OrderBy(x => x.RelativePath);
            foreach (var item in patcherFiles)
            {
                string fileHash = ComputeFileHash(item.FullPath);
                components.Add($"patchers:{item.RelativePath}:{fileHash}");
            }
        }

        string corePath = Path.Combine(root, "core");
        if (Directory.Exists(corePath))
        {
            var coreFiles = Directory.EnumerateFiles(corePath, "*.dll", SearchOption.AllDirectories)
                .Select(f => new { FullPath = f, RelativePath = GetRelativePath(root, f) })
                .OrderBy(x => x.RelativePath);
            foreach (var item in coreFiles)
            {
                string fileHash = ComputeFileHash(item.FullPath);
                components.Add($"core:{item.RelativePath}:{fileHash}");
            }
        }

        return string.Join("|", components);
    }

    private static string GetRelativePath(string root, string fullPath)
    {
        if (!fullPath.StartsWith(root))
            return fullPath;
        string relative = fullPath.Substring(root.Length);
        if (relative.StartsWith(Path.DirectorySeparatorChar) || relative.StartsWith(Path.AltDirectorySeparatorChar))
            relative = relative.Substring(1);
        return relative;
    }
}
