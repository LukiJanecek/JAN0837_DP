using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// 
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Blocks.Interface;
using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Math.EC.Endo;

using JAN0837_DP.Data;
using Siemens.Engineering.Hmi.Tag;
using System.Diagnostics;


namespace JAN0837_DP.Forms
{
    public partial class ucGenerateTIAtemplate : UserControl
    {
        TiaPortal tiaPortal;

        string tiaDLLPath = "C:\\Program Files\\Siemens\\Automation\\Portal V19\\PublicAPI\\V19"; // Siemens.Engineering.dll

        public ucGenerateTIAtemplate()
        {
            InitializeComponent();
        }

        private void ucGenerateTIAtemplate_Load(object sender, EventArgs e)
        {
            //var assembly = System.Reflection.Assembly.LoadFrom(tiaDLLPath);
        }

        private async void btnGenerateTemplate_Click(object sender, EventArgs e)
        {
            string projectName = txtParam1.Text?.Trim();

            if (string.IsNullOrWhiteSpace(projectName) )
            {
                lblStatus1.Text = "Type project name to Parameter1.";
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
                    lblStatus1.Text = "Template generated.";
                }
                else
                {
                    lblStatus1.Text = "Failed (bridge).";
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
                lblStatus1.Text = "Failed: " + ex.Message;
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

        private void btnStartTIA_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus1.Text = "Starting TIA Portal...";

                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error" + ex.Message;
            }
            finally
            {
                lblStatus1.Text = "TIA Portal started.";
            }
        }
    }
}
