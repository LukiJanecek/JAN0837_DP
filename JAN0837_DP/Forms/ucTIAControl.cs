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
//using TiaOpennessHelper;
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
using JAN0837_DP.TIA;

namespace JAN0837_DP.Forms
{
    public partial class ucTIAControl : UserControl
    {
        TiaPortal tiaPortal;
        Project projectPlc;

        string tiaDLLPath = "C:\\Program Files\\Siemens\\Automation\\Portal V19\\PublicAPI\\V19"; // Siemens.Engineering.dll
        public sealed class ProjectItem
        {
            public string Name { get; }
            public string Path { get; }
            public ProjectItem(string name, string path) { Name = name; Path = path; }
            public override string ToString() => Name; // co se bude zobrazovat v ComboBoxu
        }

        public string _selectedProjectPath;

        public ucTIAControl()
        {
            InitializeComponent();
        }

        private void ucGenerateTIAtemplate_Load(object sender, EventArgs e)
        {
            //var assembly = System.Reflection.Assembly.LoadFrom(tiaDLLPath);
            lblStatus1.Text = "Set project name and cpu type id.";
            
            lblParam1.Text = "Project name: ";
            lblParam2.Text = "CPU type ID: ";
            lblParam3.Text = "New project path: ";
            lblParam4.Text = "PLC name";

            lblDLLpath.Text = "Path to DLL project: ";
            lblTiaProject.Text = "Select TIA project: ";

            txtBoxTIADLL.Text = tiaDLLPath;
            txtBoxTIADLL.Enabled = false;
            btnChangeTIADLLPath.Enabled = false;

            //
            comboBoxTIAprojects.Items.Clear();

            try
            {
                var candidates = new[]
                {
                paths.tiaExampleProjectPath, paths.tiaSampleProjectPath
                };

                // Projdi každou podsložku – v každé očekáváme další složky s .ap* souborem
                foreach (var baseDir in candidates.Where(Directory.Exists))
                {
                    foreach (var projectDir in Directory.EnumerateDirectories(baseDir))
                    {
                        // Hledej přímo v této složce .ap* (ap19, ap20, ...). Když nic, zkus o úroveň níž.
                        var apFilesTop = Directory.EnumerateFiles(projectDir, "*.ap*", SearchOption.TopDirectoryOnly);
                        var apPath = apFilesTop.FirstOrDefault();

                        if (apPath == null)
                        {
                            // některé template projekty mají ještě podadresář se skutečným .ap souborem
                            var apFilesDeep = Directory.EnumerateFiles(projectDir, "*.ap*", SearchOption.AllDirectories);
                            apPath = apFilesDeep
                                .OrderBy(p => p.Count(c => c == System.IO.Path.DirectorySeparatorChar)) // vem nejbližší
                                .FirstOrDefault();
                        }

                        if (apPath != null)
                        {
                            var display = $"{System.IO.Path.GetFileNameWithoutExtension(apPath)}  ({System.IO.Path.GetFileName(projectDir)})";
                            comboBoxTIAprojects.Items.Add(new ProjectItem(display, apPath));
                        }
                    }
                }

                if (comboBoxTIAprojects.Items.Count > 0)
                {
                    comboBoxTIAprojects.SelectedIndex = 0;
                    _selectedProjectPath = (comboBoxTIAprojects.SelectedItem as ProjectItem).Path;
                    lblStatus1.Text = $"Found {comboBoxTIAprojects.Items.Count} project(s).";
                }
                else
                {
                    lblStatus1.Text = "No TIA projects found in Sample/Example.";
                }

            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error: " + ex.Message;
            }
        }

        private async void btnGenerateTemplate_Click(object sender, EventArgs e)
        {
            lblStatus1.Text = "Starting generating template...";

            string projectName = txtBoxParam1.Text?.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                lblStatus1.Text = "Type project name, please.";
                return;
            }

            string cputype = txtBoxParam2.Text?.Trim();

            if (string.IsNullOrWhiteSpace(cputype))
            {
                lblStatus1.Text = "Type CPU ID, please.";
                return;
            }

            var projectDir = Path.Combine(paths.tiaPath, projectName);
            Directory.CreateDirectory(projectDir);

            string args = $"--dir {projectDir} --name {projectName} --type-id {cputype} --ui";

            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "createTIAtemplate.py");

            var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, "--dir", projectDir, "--name", projectName, "--type-id", cputype, "--ui");

            if (code == 0)
            {
                lblStatus1.Text = "Template generated.";
            }
            else
            {
                lblStatus1.Text = "Template generation failed.";
            }
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

        private void comboBoxTIAprojects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTIAprojects.SelectedItem is ProjectItem item)
            {
                _selectedProjectPath = item.Path;
                lblStatus1.Text = System.IO.Path.GetFileNameWithoutExtension(item.Path);
            }
        }

        private void btnOpenProject_Click(object sender, EventArgs e)
        {
            lblStatus1.Text = "Openning TIA project.";

            try
            {
                if (string.IsNullOrWhiteSpace(_selectedProjectPath))
                {
                    lblStatus1.Text = "Please select a TIA project first.";
                    throw new InvalidOperationException("Please select a TIA project first.");
                }

                var (tiaPortal, projectPlc) = TIAcontrol.OpenOrAttachProject(_selectedProjectPath, withUI: true);
                lblStatus1.Text = $"Project open: {Path.GetFileName(_selectedProjectPath)}";
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Open error: " + ex.Message;
            }
        }

        private void btnAddDB_Click(object sender, EventArgs e)
        {
            try
            {
                if (projectPlc == null)
                {
                    if (string.IsNullOrWhiteSpace(_selectedProjectPath))
                        throw new InvalidOperationException("Open a project first (or select one).");
                    TIAcontrol.OpenOrAttachProject(_selectedProjectPath, withUI: true);
                }

                var plc = TIAcontrol.GetPlcSoftware(projectPlc);

                string dbName = "DB_ProcessData";

                TIAcontrol.CreateOrReplaceSimpleDb(plc, dbName, optimized: true);

                projectPlc.Save();

                lblStatus1.Text = $"DB '{dbName}' added.";
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error: " + ex.Message;
            }
        }

        private void btnCreateProjectPY_Click(object sender, EventArgs e)
        {

        }

        private void btnStartTIAPY_Click(object sender, EventArgs e)
        {

        }

        private void btnOpenProjectPY_Click(object sender, EventArgs e)
        {

        }

        private void btnAddDBPY_Click(object sender, EventArgs e)
        {

        }
        private void btnImportDLL_Click(object sender, EventArgs e)
        {

        }

        private void btnChangeTIADLLPath_Click(object sender, EventArgs e)
        {
            paths.tiaDLLPath = txtBoxTIADLL.Text;
            chBoxChangeTiaDLLPath.Checked = false;
            lblStatus1.Text = "Path changed successfully.";
        }

        private void chBoxChangeTiaDLLPath_CheckedChanged(object sender, EventArgs e)
        {
            if (txtBoxTIADLL.Enabled == true)
            {
                txtBoxTIADLL.Enabled = false;
                btnChangeTIADLLPath.Enabled = false;
            }
            else
            {
                txtBoxTIADLL.Enabled = true;
                btnChangeTIADLLPath.Enabled = true;
            }
        }

        private void txtBoxTIADLL_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtBoxParam1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxParam2_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnPreset_Click(object sender, EventArgs e)
        {
            txtBoxParam1.Text = "MyAwesomeTIAproject";
            txtBoxParam2.Text = "OrderNumber:6ES7 212-1AE40-0XB0/V4.6";
            txtBoxParam3.Text = paths.tiaProjectPath;
            txtBoxParam4.Text = "PLC_1";
        }
    }
}
