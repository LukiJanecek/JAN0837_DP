using JAN0837_DP.Data;
using JAN0837_DP.Log;
using MQTTnet.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.TIA
{
    public class TIAcontrol
    {
        //string tiaDLLPath = "C:\\Program Files\\Siemens\\Automation\\Portal V19\\PublicAPI\\V19"; // Siemens.Engineering.dll

        public static int? tiaPortalVersion { get; set; } = null;
        public static string _selectedProjectPath { get; set; } = "";

        public sealed class ProjectItem
        {
            public string Name { get; }
            public string Path { get; }
            public int? Version { get; }

            public ProjectItem(string name, string path)
            {
                Name = name;
                Path = path;
                Version = GetProjectVersion(path);
            }

            public override string ToString()
            {
                // Show version in display: "ProjectName (V19)"
                if (Version.HasValue)
                    return $"{Name} (V{Version.Value})";
                return Name;
            }

            public bool IsVersionMatch(int? dllVersion)
            {
                if (!Version.HasValue || !dllVersion.HasValue)
                    return true; // Unknown versions, allow

                return Version.Value == dllVersion.Value;
            }
        }

        public sealed class CPUTypeItem
        {
            public string TypeId { get; }
            public string Description { get; }

            public CPUTypeItem(string typeId, string description)
            {
                TypeId = typeId;
                Description = description;
            }

            public override string ToString()
            {
                // Show user-friendly display
                return $"{TypeId} ({Description})";
            }
        }

        public static async Task<(int code, string stdout, string stderr)> runPY(string pythonexe, string scriptPath, params string[] args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = pythonexe, // 64-bit Python
                //Arguments = $"{scriptPath} {string.Join(" ", args)}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? Environment.CurrentDirectory
            };

            psi.ArgumentList.Add(scriptPath);
            foreach (var a in args)
                psi.ArgumentList.Add(a);

            psi.Environment["PYTHONUTF8"] = "1";

            using var p = Process.Start(psi)!;
            var so = await p.StandardOutput.ReadToEndAsync();
            var se = await p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();

            return (p.ExitCode, so, se);
        }

        public static int? DetectTIAVersion(string dllPath)
        {
            if (string.IsNullOrWhiteSpace(dllPath))
                return null;

            try
            {
                // Find ALL occurrences of V## pattern, take the LAST one
                var matches = System.Text.RegularExpressions.Regex.Matches(dllPath, @"[Vv](\d+)");
                if (matches.Count > 0)
                {
                    // Take the last match (e.g., V17 instead of V19 from "Portal V19\...\V17")
                    var lastMatch = matches[matches.Count - 1];
                    if (int.TryParse(lastMatch.Groups[1].Value, out int version))
                    {
                        return version;
                    }
                }
                
                return null;
            }
            catch
            {
                Logger.LogError($"Failed to detect TIA version from DLL path: {dllPath}");
                return null;
            }
        }

        public static int? GetProjectVersion(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return null;

            try
            {
                var extension = Path.GetExtension(projectPath);
                if (string.IsNullOrEmpty(extension))
                    return null;

                // Extract number from extension like ".ap19"
                var match = System.Text.RegularExpressions.Regex.Match(extension, @"\.ap(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int version))
                {
                    return version;
                }

                return null;
            }
            catch
            {
                Logger.LogError($"Failed to get project version from path: {projectPath}");
                return null;
            }
        }
    }
}
