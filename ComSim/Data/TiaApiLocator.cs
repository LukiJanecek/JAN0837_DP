using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace JAN0837_DP.Data;

/// <summary>Locates either the legacy monolithic or the V21+ modular Openness API.</summary>
public static class TiaApiLocator
{
    private static readonly string[] ApiMarkers =
    {
        "Siemens.Engineering.Base.dll",
        "Siemens.Engineering.dll"
    };

    public static string? FindBestInstalledPath()
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddRegistryCandidates(candidates, RegistryView.Registry64);
        AddRegistryCandidates(candidates, RegistryView.Registry32);

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var automationRoot = Path.Combine(programFiles, "Siemens", "Automation");
        if (Directory.Exists(automationRoot))
        {
            foreach (var portal in Directory.EnumerateDirectories(automationRoot, "Portal V*"))
                candidates.Add(portal);
        }

        return candidates
            .SelectMany(FindApiDirectories)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(GetVersion)
            .ThenByDescending(p => File.Exists(Path.Combine(p, "Siemens.Engineering.Base.dll")))
            .FirstOrDefault();
    }

    /// <summary>Accepts a DLL, its directory, or a PublicAPI parent and returns the actual assembly directory.</summary>
    public static string? ResolveApiDirectory(string? selectedPath)
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
            return null;

        var path = File.Exists(selectedPath) ? Path.GetDirectoryName(selectedPath)! : selectedPath;
        return FindApiDirectories(path).OrderByDescending(GetVersion).FirstOrDefault();
    }

    private static IEnumerable<string> FindApiDirectories(string root)
    {
        if (!Directory.Exists(root))
            yield break;

        if (ApiMarkers.Any(marker => File.Exists(Path.Combine(root, marker))))
        {
            yield return Path.GetFullPath(root);
            yield break;
        }

        string[] directories;
        try
        {
            directories = Directory.GetDirectories(root, "*", SearchOption.AllDirectories);
        }
        catch
        {
            yield break;
        }

        foreach (var directory in directories)
        {
            if (ApiMarkers.Any(marker => File.Exists(Path.Combine(directory, marker))))
                yield return Path.GetFullPath(directory);
        }
    }

    private static void AddRegistryCandidates(HashSet<string> candidates, RegistryView view)
    {
        try
        {
            using var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            using var openness = hive.OpenSubKey(@"SOFTWARE\Siemens\Automation\Openness");
            if (openness == null) return;

            foreach (var versionName in openness.GetSubKeyNames())
            {
                using var version = openness.OpenSubKey(versionName);
                using var publicApi = version?.OpenSubKey("PublicAPI");
                if (publicApi == null) continue;

                AddPathValue(publicApi, candidates);
                foreach (var apiVersionName in publicApi.GetSubKeyNames())
                {
                    using var apiVersion = publicApi.OpenSubKey(apiVersionName);
                    if (apiVersion != null) AddPathValue(apiVersion, candidates);
                }
            }
        }
        catch
        {
            // Registry discovery is optional; the UI still permits manual selection.
        }
    }

    private static void AddPathValue(RegistryKey key, HashSet<string> candidates)
    {
        foreach (var valueName in key.GetValueNames())
        {
            if (key.GetValue(valueName) is string value && !string.IsNullOrWhiteSpace(value))
                candidates.Add(File.Exists(value) ? Path.GetDirectoryName(value)! : value);
        }
        if (key.GetValue(null) is string defaultValue && !string.IsNullOrWhiteSpace(defaultValue))
            candidates.Add(File.Exists(defaultValue) ? Path.GetDirectoryName(defaultValue)! : defaultValue);
    }

    private static int GetVersion(string path)
    {
        var matches = Regex.Matches(path, @"[Vv](\d+)");
        return matches.Count > 0 && int.TryParse(matches[^1].Groups[1].Value, out var version)
            ? version
            : 0;
    }
}
