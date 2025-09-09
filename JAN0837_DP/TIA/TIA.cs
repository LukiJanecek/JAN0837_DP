using JAN0837_DP.Data;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW;
using Siemens.Engineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JAN0837_DP.TIA
{
    public class TIA
    {
        TiaPortal tiaPortal;
        public void GenerateTemplate(string name, string label)
        {
            if (name != "" || name != null)
            {
                label = "Generating template in TIA.";

                string tiaProjectFolder = Path.Combine(paths.tiaPath, name);

                Directory.CreateDirectory(tiaProjectFolder);
                var projectFolderInfo = new DirectoryInfo(tiaProjectFolder);

                label = "Running TIA Portal.";

                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);

                label = "Creating folder with project.";

                Project project = tiaPortal.Projects.Create(projectFolderInfo, name);
                var devices = project.Devices;
                //object deviceItemRef;

                label = "Adding PLC.";
                var device = devices.CreateWithItem("CPU_1212C_DC_DC_DC", "V4.5", name);

                // CPU module
                DeviceItem plcDeviceItem = device.DeviceItems[0];

                // PLC software
                var softwareContainer = plcDeviceItem.GetService<SoftwareContainer>();
                var plcSoftware = softwareContainer.Software as PlcSoftware;

                // Data block 
                label = "Adding FB + DB.";
                var blockGroup = plcSoftware.BlockGroup;
                var fb = blockGroup.Blocks.CreateFB("FB_test", true, 1, ProgrammingLanguage.LAD);
                var db = blockGroup.Blocks.CreateInstanceDB("test", true, 1, "FB_test");

                project.Save();
                tiaPortal.Dispose();

                label = "Template generated.";
            }
            else
            {
                label = "Type project name to Parameter1.";
            }
        }

        public void GenerateProject(string name, string label)
        {
            string projectName = name; //.Text?.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                label = "Type project name to Parameter1.";
                return;
            }

            try
            {
                label = "Generating template in TIA....";

                using var tia = new TiaPortal(TiaPortalMode.WithUserInterface);

                // creating directory and paths 
                label = "Creating project...";
                string tiaProjectFolder = Path.Combine(paths.tiaPath, projectName);
                Directory.CreateDirectory(tiaProjectFolder);
                var projectFolderInfo = new DirectoryInfo(tiaProjectFolder);
                var project = tia.Projects.Create(projectFolderInfo, projectName);
                label = "Project created.";

                // adding plc 
                label = "Adding PLC...";
                var device = project.Devices.CreateWithItem("OrderNumber:6ES7 212-1BD34-0XB0/V4.5", "PLC_1", "PLC_" + projectName);

                var cpuItem = device.DeviceItems.OfType<DeviceItem>().FirstOrDefault(di => di.GetService<SoftwareContainer>() != null) ?? throw new InvalidOperationException("CPU software not found.");

                var plc = (PlcSoftware)cpuItem.GetService<SoftwareContainer>().Software;


                project.Save();
                label = "Template generated.";
            }
            catch (Exception ex)
            {
                label = "Failed: " + ex.Message;
            }
            finally
            {

            }
        }

        public async void generate(string name, string label)
        {
            string projectName = name; // txtParam1.Text?.Trim()

            if (string.IsNullOrWhiteSpace(projectName))
            {
                label = "Type project name to Parameter1.";
                return;
            }

            try
            {
                //lblStatus1.Text = "Generating template in TIA....";
                //using var tia = new TiaPortal(TiaPortalMode.WithUserInterface);

                // creating directory and paths 
                //lblStatus1.Text = "Creating project...";
                string tiaProjectFolder = Path.Combine(paths.tiaPath, projectName);
                Directory.CreateDirectory(tiaProjectFolder);

                string cpuTypeId = @"OrderNumber:6ES7 212-1BD34-0XB0/V4.5";

                //var projectFolderInfo = new DirectoryInfo(tiaProjectFolder);

                var (exitCode, stdOut, stdErr) = await RunOpennessBridgeAsync(tiaProjectFolder, projectName, cpuTypeId, withUI: true);

                if (exitCode == 0)
                {
                    label = "Template generated.";
                }
                else
                {
                    label = "Failed (bridge).";
                    MessageBox.Show(
                        $"OpennessBridge failed ({exitCode}).\n\nSTDOUT:\n{stdOut}\n\nSTDERR:\n{stdErr}",
                        "TIA Openness",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                label = "Failed: " + ex.Message;
                MessageBox.Show(ex.ToString(), "TIA Openness", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<(int exitCode, string stdOut, string stdErr)> RunOpennessBridgeAsync(string projectDir, string projectName, string cpuTypeId, bool withUI)
        {
            // Umísti OpennessBridge.exe vedle tvé .NET 8 aplikace (nebo změň cestu)
            string bridgePath = Path.Combine(AppContext.BaseDirectory, "OpennessBridge.exe");
            if (!File.Exists(bridgePath))
                throw new FileNotFoundException("OpennessBridge.exe not found next to the app.", bridgePath);

            static string Q(string s) => "\"" + s.Replace("\"", "\\\"") + "\"";

            // args: gen <dir> <name> <typeId> --ui/--no-ui
            string args = new StringBuilder()
                .Append("gen ").Append(Q(projectDir)).Append(' ')
                .Append(Q(projectName)).Append(' ')
                .Append(Q(cpuTypeId)).Append(' ')
                .Append(withUI ? "--ui" : "--no-ui")
                .ToString();

            var psi = new ProcessStartInfo
            {
                FileName = bridgePath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(bridgePath)!
            };

            using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.Start();

            var stdOutTask = p.StandardOutput.ReadToEndAsync();
            var stdErrTask = p.StandardError.ReadToEndAsync();

            await Task.WhenAll(stdOutTask, stdErrTask, p.WaitForExitAsync());
            return (p.ExitCode, stdOutTask.Result, stdErrTask.Result);
        }
    }
}
