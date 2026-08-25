using Microsoft.Win32;

namespace JAN0837_DP.Data;

public static class PythonRuntimeLocator
{
    public static string? FindPythonExecutable(string scriptsFolder)
    {
        var venvPython = Path.Combine(scriptsFolder, "venv", "Scripts", "python.exe");
        if (IsUsableVirtualEnvironment(venvPython))
            return venvPython;

        foreach (var candidate in FindSystemPythonCandidates())
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static bool IsUsableVirtualEnvironment(string pythonExecutable)
    {
        if (!File.Exists(pythonExecutable)) return false;

        var configPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(pythonExecutable)!, "..", "..", "pyvenv.cfg"));
        if (!File.Exists(configPath)) return true;

        try
        {
            var baseExecutable = File.ReadLines(configPath)
                .FirstOrDefault(line => line.StartsWith("executable", StringComparison.OrdinalIgnoreCase))?
                .Split('=', 2)[1].Trim();
            return string.IsNullOrWhiteSpace(baseExecutable) || File.Exists(baseExecutable);
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<string> FindSystemPythonCandidates()
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            yield return Path.Combine(directory.Trim().Trim('"'), "python.exe");

        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            RegistryKey? root = null;
            try
            {
                root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view)
                    .OpenSubKey(@"SOFTWARE\Python\PythonCore");
                if (root == null) continue;
                foreach (var version in root.GetSubKeyNames().OrderByDescending(v => v))
                {
                    using var installPath = root.OpenSubKey(version + @"\InstallPath");
                    if (installPath?.GetValue("ExecutablePath") is string executable)
                        yield return executable;
                    if (installPath?.GetValue(null) is string directory)
                        yield return Path.Combine(directory, "python.exe");
                }
            }
            finally
            {
                root?.Dispose();
            }
        }
    }
}
