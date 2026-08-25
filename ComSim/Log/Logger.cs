using System.Diagnostics;
using JAN0837_DP.Data;

namespace JAN0837_DP.Log;

public static class Logger
{
    private static readonly object SyncRoot = new();

    public static void LogInfo(string message) => Write("INFO", message);

    public static void LogWarning(string message) => Write("WARN", message);

    public static void LogError(string message) => Write("ERROR", message);

    public static void LogException(Exception exception, string context = "")
    {
        ArgumentNullException.ThrowIfNull(exception);
        Write("ERROR", $"{context}{exception}");
    }

    private static void Write(string level, string message)
    {
        var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
        Debug.WriteLine(entry);

        try
        {
            lock (SyncRoot)
            {
                Directory.CreateDirectory(paths.logDirectoryPath);
                File.AppendAllText(paths.logFilePath, entry + Environment.NewLine);
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Logger failed to write to disk: {exception}");
        }
    }
}
