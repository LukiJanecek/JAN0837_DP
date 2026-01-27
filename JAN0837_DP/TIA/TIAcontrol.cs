using JAN0837_DP.Data;
using MQTTnet.Internal;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
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
        public static int? tiaPortalVersion { get; set; } = null;

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

        public static (TiaPortal portal, Project project) OpenOrAttachProject(string projectPath, bool withUI = true)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !File.Exists(projectPath))
            {
                throw new FileNotFoundException("TIA project file not found.", projectPath);
            }

            // try connecting to running TIA Portal process
            try
            {
                foreach(var p in TiaPortal.GetProcesses())
                {
                    if (p.ProjectPath?.FullName?.Equals(projectPath, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var attached = p.Attach();
                        return (attached, attached.Projects[0]);
                    }
                }
            }   
            catch (Exception ex)
            {
                // could not attach, proceed to open a new instanceq
            }

            var tia = new TiaPortal(TiaPortalMode.WithUserInterface);
            tia.Projects.Open(new FileInfo(projectPath));
            return (tia, tia.Projects[0]);
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
                return null;
            }
        }
    }
}
