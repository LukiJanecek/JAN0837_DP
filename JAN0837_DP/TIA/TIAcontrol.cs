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
        TiaPortal tiaPortal;
        Project projectPlc;

        //public ProjectStructrTree[] ProjectTree { get; set; }

        //public ProjectStructrTree[] ProgramBlocks { get; set; }

        public string[] plcNames { get; set; }

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

        public static async Task<(int exitCode, string stdOut, string stdErr)> RunOpennessBridgeAsync(string projectDir, string projectName, string cpuTypeId, bool withUI)
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
                

            // Zkusit se připojit na běžící procesy
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
                
            }

            var tia = new TiaPortal(TiaPortalMode.WithUserInterface);
            tia.Projects.Open(new FileInfo(projectPath));
            return (tia, tia.Projects[0]);
        }

        public static PlcSoftware GetPlcSoftware(Project project, string deviceName = null)
        {
            foreach (var dev in project.Devices)
            {
                // CPU item = ten, který má SoftwareContainer
                var cpu = dev.DeviceItems
                    .OfType<DeviceItem>()
                    .FirstOrDefault(di => di.GetService<SoftwareContainer>()?.Software is PlcSoftware);

                if (cpu != null)
                {
                    if (string.IsNullOrWhiteSpace(deviceName) || dev.Name.Equals(deviceName, StringComparison.OrdinalIgnoreCase))
                        return (PlcSoftware)cpu.GetService<SoftwareContainer>().Software;
                }
            }
            throw new InvalidOperationException("PLC software not found in project.");
        }

        public static void CreateOrReplaceSimpleDb(PlcSoftware plc, string dbName, bool optimized = true)
        {
            var attr = optimized ? "{ S7_Optimized_Access := 'TRUE' }" : "{ S7_Optimized_Access := 'FALSE' }";

            string dbScl =
                $@"DATA_BLOCK {dbName}
                {attr}
                VERSION : 0.1
                  VAR
                    Speed       : Real := 0.0;
                    Count       : DInt := 0;
                    Enabled     : Bool := FALSE;
                    Timestamp   : Date_And_Time := DT#1970-01-01-00:00:00;
                  END_VAR
                BEGIN
                END_DATA_BLOCK";

            var existing = plc.BlockGroup.Blocks.OfType<PlcBlock>().FirstOrDefault(b => b.Name == dbName);
            if (existing != null) existing.Delete();

            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"DB_{dbName}.scl");
            System.IO.File.WriteAllText(tmp, dbScl, System.Text.Encoding.UTF8);

            PlcExternalSourceGroup srcGroup = plc.ExternalSourceGroup; 
            PlcExternalSource src = srcGroup.ExternalSources.CreateFromFile($"DB_{dbName}.scl", tmp);
            src.GenerateBlocksFromSource();
            File.Delete(tmp);
        }

        // 
        /*
        private bool GenerateCounterPlcProject(Project project)
        {
            if (project != null)
            {
                var devices = OpennessHelper.GetAllPlcSoftwares(projectPlc).Concat(OpennessHelper.GetAllPlcSoftwaresInGroups(projectPlc));
                foreach (var plcSoftware in devices)
                {
                    plcNames.Append(plcSoftware.Name);
                }
                if (!plcNames.Length.Equals(0))
                {
                    plcNames = null;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private void GenerateTreeViewPlc(string cbText)
        {
            Dictionary<string, ProjectStructrTree> projectTree = new Dictionary<string, ProjectStructrTree>();
            var devices = OpennessHelper.GetAllPlcSoftwares(projectPlc).Concat(OpennessHelper.GetAllPlcSoftwaresInGroups(projectPlc));
            foreach (var plcSoftware in devices)
            {
                if (plcSoftware.Name == cbText)
                {
                    #region Program Blocks                                    
                    foreach (var plcBlock in plcSoftware.BlockGroup.Blocks)
                    {
                        ProjectStructrTree pTree = new ProjectStructrTree
                        {
                            Name = plcBlock.Name,
                            Tag = plcBlock,
                        };
                        projectTree.Add(pTree.Name, pTree);
                    }
                    ProjectStructrTree plcTree = new ProjectStructrTree();
                    ListRecursivePlcGroups(plcTree.Items, plcSoftware.BlockGroup.Groups);
                    foreach (var item in plcTree.Items)
                    {
                        int index = plcTree.Items.IndexOf(item);
                        string actItem = item.Name.ToString();

                        // Check if projectTree cointains the same key as plcTree
                        if (!projectTree.ContainsKey(actItem))
                        {
                            projectTree.Add(plcTree.Items[index].Name, plcTree.Items[index]);
                        }
                        else
                        {
                            // Do nothing
                        }
                    }
                    #endregion

                    List<ProjectStructrTree> ret = new List<ProjectStructrTree>();
                    foreach (ProjectStructrTree t in projectTree.Values)
                    {
                        ret.Add(t);
                    }
                    ProgramBlocks = ret.ToArray();
                }
            }

        }

        private void ListRecursivePlcGroups(List<ProjectStructrTree> projectTree, IEnumerable<PlcBlockGroup> item)
        {
            foreach (var itemGroup in item)
            {
                ProjectStructrTree pTree = new ProjectStructrTree
                {
                    Name = itemGroup.Name,
                    Tag = itemGroup,
                };
                foreach (var itemBlock in itemGroup.Blocks)
                {
                    ProjectStructrTree pTree1 = new ProjectStructrTree
                    {
                        Name = itemBlock.Name,
                        Tag = itemBlock,
                    };
                    pTree.Items.Add(pTree1);
                }
                projectTree.Add(pTree);
                ListRecursivePlcGroups(pTree.Items, itemGroup.Groups);
            }
        }
        */
    }
}
