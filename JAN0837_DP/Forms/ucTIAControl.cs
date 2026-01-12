using JAN0837_DP.Data;
using JAN0837_DP.TIA;
using Org.BouncyCastle.Math.EC.Endo;
using S7.Net;
// 
//using TiaOpennessHelper;
using Siemens.Engineering;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks.Interface;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JAN0837_DP.Forms
{
    public partial class ucTIAControl : UserControl
    {
        TiaPortal tiaPortal;
        Project projectPlc;

        //string tiaDLLPath = "C:\\Program Files\\Siemens\\Automation\\Portal V19\\PublicAPI\\V19"; // Siemens.Engineering.dll
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

        private void ucTIAControl_Load(object sender, EventArgs e)
        {
            //var assembly = System.Reflection.Assembly.LoadFrom(tiaDLLPath);

            // UI settings
            #region UI settings

            lblStatus1.Text = "";

            // params labels
            lblParam1.Text = "";
            lblParam2.Text = "";
            lblParam3.Text = "";
            lblParam4.Text = "";

            lblDLLpath.Text = "Path to Siemens.Engineering.dll:";
            lblTiaProject.Text = "Select TIA Portal project:";

            txtBoxTIADLL.Enabled = false;
            txtBoxTIADLL.Text = paths.tiaDLLPath;
            btnImportDLL.Enabled = false;
            btnImportDLL.Visible = true;

            comboBoxTIAprojects.Enabled = false;
            comboBoxTIAprojects.Visible = false;

            // rbtns
            rbtnCreateNewProject.Enabled = true;
            rbtnCreateNewProject.Visible = true;
            rbtnCreateNewProject.Checked = false;

            rbtnOpenProject.Enabled = true;
            rbtnOpenProject.Visible = true;
            rbtnOpenProject.Checked = false;

            // btns 
            btnStartTIA.Enabled = true;
            btnStartTIA.Visible = true;

            btnPreset.Enabled = false;
            btnPreset.Visible = false;

            btnAddDB.Enabled = false;
            btnAddDB.Visible = false;

            btnOpenProject.Enabled = false;
            btnOpenProject.Visible = false;

            btnCreateProject.Enabled = false;
            btnCreateProject.Visible = false;

            btnFindTIAProjectOnPath.Enabled = false;
            btnFindTIAProjectOnPath.Visible = false;

            // lbls
            lblTiaProject.Visible = false;
            lblParam1.Visible = false;
            lblParam2.Visible = false;
            lblParam2.Visible = false;
            lblParam3.Visible = false;
            lblParam3.Visible = false;
            lblParam4.Visible = false;
            lblParam4.Visible = false;

            // txtBoxes
            txtBoxParam1.Enabled = false;
            txtBoxParam1.Visible = false;
            txtBoxParam1.Clear();
            txtBoxParam2.Enabled = false;
            txtBoxParam2.Visible = false;
            txtBoxParam2.Clear();
            txtBoxParam3.Enabled = false;
            txtBoxParam3.Visible = false;
            txtBoxParam3.Clear();
            txtBoxParam4.Enabled = false;
            txtBoxParam4.Visible = false;
            txtBoxParam4.Clear();

            #endregion
        }

        private void FindMyProjectInThisProject()
        {
            comboBoxTIAprojects.Items.Clear();

            try
            {
                // prefer the main TIA_projects root; fallback to Sample/Example if missing
                var searchRoots = new List<string>();
                if (!string.IsNullOrWhiteSpace(paths.tiaProjectPath) && Directory.Exists(paths.tiaProjectPath))
                {
                    searchRoots.Add(paths.tiaProjectPath);
                }
                    

                if (Directory.Exists(paths.tiaSampleProjectPath))
                {
                    searchRoots.Add(paths.tiaSampleProjectPath);
                }
                    

                if (Directory.Exists(paths.tiaExampleProjectPath))
                {
                    searchRoots.Add(paths.tiaExampleProjectPath);
                }
                   

                if (searchRoots.Count == 0)
                {
                    lblStatus1.Text = "No TIA projects root folder found.";
                    return;
                }

                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var found = new List<ProjectItem>();

                // search all .ap* files under each root (recursive)
                foreach (var root in searchRoots)
                {
                    try
                    {
                        var apFiles = Directory.EnumerateFiles(root, "*.ap*", SearchOption.AllDirectories);
                        foreach (var apPath in apFiles)
                        {
                            var full = Path.GetFullPath(apPath);
                            if (!seen.Add(full)) continue; // avoid duplicates

                            var fileName = Path.GetFileNameWithoutExtension(apPath);
                            var parentDir = Path.GetFileName(Path.GetDirectoryName(apPath)) ?? "";
                            var display = string.IsNullOrEmpty(parentDir) ? fileName : $"{fileName}  ({parentDir})";

                            found.Add(new ProjectItem(display, apPath));
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // skip roots we can't access
                        continue;
                    }
                }

                if (found.Count > 0)
                {
                    foreach (var item in found.OrderBy(p => p.Name))
                        comboBoxTIAprojects.Items.Add(item);

                    comboBoxTIAprojects.SelectedIndex = 0;
                    _selectedProjectPath = (comboBoxTIAprojects.SelectedItem as ProjectItem).Path;
                    lblStatus1.Text = $"Found {comboBoxTIAprojects.Items.Count} project(s). Choose one and open it or add pre-prepared data block in it.";
                }
                else
                {
                    lblStatus1.Text = "No TIA projects found under the configured folder(s).";
                }
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error: " + ex.Message;
            }
        }

        // rbtns
        #region rbtns
        private void rbtnOpenProject_CheckedChanged(object sender, EventArgs e)
        {
            // UI settings 
            #region UI settings

            //
            comboBoxTIAprojects.Enabled = true;
            comboBoxTIAprojects.Visible = true;

            btnOpenProject.Enabled = true;
            btnOpenProject.Visible = true;

            btnAddDB.Enabled = true;
            btnAddDB.Visible = true;

            btnFindTIAProjectOnPath.Enabled = true;
            btnFindTIAProjectOnPath.Visible = true;

            txtBoxParam1.Enabled = true;
            txtBoxParam1.Visible = true;
            txtBoxParam1.Text = paths.tiaProjectPath;

            lblTiaProject.Visible = true;

            lblParam1.Visible = true;
            lblParam1.Text = "Type your project path: ";

            //

            txtBoxParam2.Enabled = false;
            txtBoxParam2.Visible = false;

            txtBoxParam3.Enabled = false;
            txtBoxParam3.Visible = false;

            txtBoxParam4.Enabled = false;
            txtBoxParam4.Visible = false;

            btnPreset.Enabled = false;
            btnPreset.Visible = false;

            btnCreateProject.Enabled = false;
            btnCreateProject.Visible = false;

            //lblParam1.Visible = false;
            lblParam2.Visible = false;
            lblParam3.Visible = false;
            lblParam4.Visible = false;

            #endregion

            FindMyProjectInThisProject();
        }

        private void rbtnCreateNewProject_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus1.Text = "Type your inputs and create new template project.";

            // UI settings 
            #region UI settings 

            //
            txtBoxParam1.Enabled = true;
            txtBoxParam1.Visible = true;
            txtBoxParam1.Text = "";

            txtBoxParam2.Enabled = true;
            txtBoxParam2.Visible = true;

            txtBoxParam3.Enabled = true;
            txtBoxParam3.Visible = true;

            txtBoxParam4.Enabled = true;
            txtBoxParam4.Visible = true;

            btnPreset.Enabled = true;
            btnPreset.Visible = true;

            btnCreateProject.Enabled = true;
            btnCreateProject.Visible = true;

            lblParam1.Visible = true;
            lblParam2.Visible = true;
            lblParam3.Visible = true;
            lblParam4.Visible = true;

            lblParam1.Text = "Project name: ";
            lblParam2.Text = "CPU type ID: ";
            lblParam3.Text = "New project path: ";
            lblParam4.Text = "PLC name:";

            //
            comboBoxTIAprojects.Enabled = false;
            comboBoxTIAprojects.Visible = false;

            btnOpenProject.Enabled = false;
            btnOpenProject.Visible = false;

            btnAddDB.Enabled = false;
            btnAddDB.Visible = false;

            btnFindTIAProjectOnPath.Enabled = false;
            btnFindTIAProjectOnPath.Visible = false;

            lblTiaProject.Visible = false;

            #endregion
        }
        #endregion

        // txtBoxes
        #region txtBoxes
        private void txtBoxTIADLL_TextChanged(object sender, EventArgs e)
        {
            paths.tiaDLLPath = txtBoxTIADLL.Text;
        }
        private void txtBoxParam1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxParam2_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxParam3_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxParam4_TextChanged(object sender, EventArgs e)
        {

        }

        #endregion

        private void comboBoxTIAprojects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTIAprojects.SelectedItem is ProjectItem item)
            {
                _selectedProjectPath = item.Path;
                lblStatus1.Text = System.IO.Path.GetFileNameWithoutExtension(item.Path);
            }
        }

        private void chBoxChangeTiaDLLPath_CheckedChanged(object sender, EventArgs e)
        {
            if (txtBoxTIADLL.Enabled == true)
            {
                txtBoxTIADLL.Enabled = false;
                btnImportDLL.Enabled = false;
            }
            else
            {
                txtBoxTIADLL.Enabled = true;
                btnImportDLL.Enabled = true;
            }
        }

        // btns
        #region btns

        private void btnStartTIA_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus1.Text = "Starting TIA Portal...";

                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);

                lblStatus1.Text = "TIA Portal started.";
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error" + ex.Message;
            }
        }

        private async void btnOpenProject_Click(object sender, EventArgs e)
        {
            // this doesnt work properly -> python will do this :) 
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedProjectPath))
                {
                    lblStatus1.Text = "Please select a TIA project first.";
                    throw new InvalidOperationException("Please select a TIA project first.");
                }

                lblStatus1.Text = "Openning TIA project.";

                var (tiaPortal, projectPlc) = TIAcontrol.OpenOrAttachProject(_selectedProjectPath, withUI: true);

                lblStatus1.Text = $"Project opened: {Path.GetFileName(_selectedProjectPath)}";
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error: " + ex.Message;
            }

            // py 
            if (string.IsNullOrWhiteSpace(_selectedProjectPath))
            {
                lblStatus1.Text = "Please select a TIA project first.";
                throw new InvalidOperationException("Please select a TIA project first.");
            }

            string[] args = new[] { "--dll-dir", paths.tiaDLLPath, "--project-dir", _selectedProjectPath, "--ui" };

            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "openPathProject.py"); // 

            lblStatus1.Text = "Openning TIA project.";

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

                // Show full diagnostic info when script fails
                if (code != 0)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine($"Python exit code: {code}");
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        msg.AppendLine("=== STDOUT ===");
                        msg.AppendLine(stdout);
                    }
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        msg.AppendLine("=== STDERR ===");
                        msg.AppendLine(stderr);
                    }

                    // msg.ToString();
                    lblStatus1.Text = $"Openning project failed, please check your path to Siemens.Engineering.dll.";
                    return;
                }

                // Success: optionally show stdout
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    lblStatus1.Text = "Project opened successfuly: " + stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                else
                {
                    lblStatus1.Text = "Project opened successfuly.";
                }
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Code exception error, openning project failed: " + ex.Message;
            }

        }

        private async void btnCreateProject_Click(object sender, EventArgs e)
        {
            string projectName = txtBoxParam1.Text.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                lblStatus1.Text = "Type project name, please.";
                return;
            }

            string cputype = txtBoxParam2.Text.Trim();

            if (string.IsNullOrWhiteSpace(cputype))
            {
                lblStatus1.Text = "Type CPU ID, please.";
                return;
            }

            string projectPath = txtBoxParam3.Text.Trim();

            if (string.IsNullOrWhiteSpace(projectPath))
            {
                lblStatus1.Text = "Type new project path, please.";
                return;
            }

            string plcName = txtBoxParam4.Text.Trim();
            if (string.IsNullOrWhiteSpace(plcName))
            {
                lblStatus1.Text = "Type PLC name, please.";
                return;
            }

            // ensure directory exists 
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }

            lblStatus1.Text = "Starting generating template...";

            //string arg = $"--dir {projectPath} --name {projectName} --type-id {cputype} --plc-name {plcName} --ui";
            string[] args = new[] { "--dll-dir", paths.tiaDLLPath, "--project-dir", projectPath, "--project-name", projectName, "--type-id", cputype, "--plc-name", plcName, "--ui" };

            ////
            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "createNewTIAPortalProject.py"); 

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

                // Show full diagnostic info when script fails
                if (code != 0)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine($"Python exit code: {code}");
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        msg.AppendLine("=== STDOUT ===");
                        msg.AppendLine(stdout);
                    }
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        msg.AppendLine("=== STDERR ===");
                        msg.AppendLine(stderr);
                    }

                    // msg.ToString();
                    lblStatus1.Text = $"Creating new project failed, please check your path to Siemens.Engineering.dll.";
                    return;
                }

                // Success: optionally show stdout
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    lblStatus1.Text = "Project created successfuly: " + stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                else
                {
                    lblStatus1.Text = "Project created successfuly.";
                }
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Code exception error, creating project failed: " + ex.Message;
            }
        }

        private async void btnAddDB_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedProjectPath))
            {
                lblStatus1.Text = "Please select a TIA project first.";
                throw new InvalidOperationException("Please select a TIA project first.");
            }

            ////
            string[] args = new[] { "--dll-dir", paths.tiaDLLPath, "--project-dir", _selectedProjectPath , "--ui" };

            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "addDBtoPathProject.py"); 

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

                // Show full diagnostic info when script fails
                if (code != 0)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine($"Python exit code: {code}");
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        msg.AppendLine("=== STDOUT ===");
                        msg.AppendLine(stdout);
                    }
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        msg.AppendLine("=== STDERR ===");
                        msg.AppendLine(stderr);
                    }

                    // msg.ToString();
                    lblStatus1.Text = $"Adding DB to project failed, please check your path to Siemens.Engineering.dll.";
                    return;
                }

                // Success: optionally show stdout
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    lblStatus1.Text = "DB added successfuly: " + stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                else
                {
                    lblStatus1.Text = "DB added successfuly.";
                }
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Code exception error, adding DB failed: " + ex.Message;
            }
        }

        private async void btnImportDLL_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBoxTIADLL.Text.Trim()))
            {
                lblStatus1.Text = $"Type path to text box under text '{lblDLLpath.Text}', please.";
                return;
            }

            // test import on current path in txtBoxTIADLL
            string[] args = new[] { "--dir", txtBoxTIADLL.Text.Trim() };

            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "importTIADLL.py"); 
            /*
            if (!File.Exists(paths.pythonExePath))
            {
                MessageBox.Show($"Python executable not found:\n{paths.pythonExePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(pythonScriptPath))
            {
                MessageBox.Show($"Python script not found:\n{pythonScriptPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            */

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

                // Show full diagnostic info when script fails
                if (code != 0)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine($"Python exit code: {code}");
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        msg.AppendLine("=== STDOUT ===");
                        msg.AppendLine(stdout);
                    }
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        msg.AppendLine("=== STDERR ===");
                        msg.AppendLine(stderr);
                    }

                    // msg.ToString();
                    lblStatus1.Text = $"Import failed, please check your path to Siemens.Engineering.dll.";
                    return;
                }

                // Success: optionally show stdout
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    lblStatus1.Text = "Import Siemens.Engineering.dll successful: " + stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                else
                {
                    lblStatus1.Text = "Import Siemens.Engineering.dll successful.";
                }

                paths.tiaDLLPath = txtBoxTIADLL.Text.Trim();
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Code exception error, import failed: " + ex.Message;
            }
        }

        private void btnPreset_Click(object sender, EventArgs e)
        {
            txtBoxParam1.Text = "MyAwesomeTIAproject";
            txtBoxParam2.Text = "OrderNumber:6ES7 212-1AE40-0XB0/V4.6";
            txtBoxParam3.Text = paths.tiaProjectPath;
            txtBoxParam4.Text = "PLC_1";
        }

        private void btnFindTIAProjectOnPath_Click(object sender, EventArgs e)
        {
            comboBoxTIAprojects.Items.Clear();

            var inputPath = txtBoxParam1.Text?.Trim();

            if (string.IsNullOrEmpty(inputPath))
            {
                lblStatus1.Text = "Please enter a folder path in the input first.";
                return;
            }

            try
            {
                // If user specified a file directly, accept it when it's a .ap* file
                if (System.IO.File.Exists(inputPath))
                {
                    if (System.IO.Path.GetExtension(inputPath).StartsWith(".ap", StringComparison.OrdinalIgnoreCase))
                    {
                        var display = System.IO.Path.GetFileNameWithoutExtension(inputPath);
                        comboBoxTIAprojects.Items.Add(new ProjectItem(display, inputPath));
                        comboBoxTIAprojects.SelectedIndex = 0;
                        _selectedProjectPath = inputPath;
                        lblStatus1.Text = $"Found {comboBoxTIAprojects.Items.Count} project file.";
                    }
                    else
                    {
                        lblStatus1.Text = "Specified file is not a .ap* project file.";
                    }

                    return;
                }

                // Path must be an existing directory from here
                if (!System.IO.Directory.Exists(inputPath))
                {
                    lblStatus1.Text = "Directory does not exist: " + inputPath;
                    return;
                }

                // Search recursively for any .ap* files under the given folder
                var apFiles = System.IO.Directory.EnumerateFiles(inputPath, "*.ap*", System.IO.SearchOption.AllDirectories);

                var found = new List<ProjectItem>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var apPath in apFiles)
                {
                    var full = System.IO.Path.GetFullPath(apPath);
                    if (!seen.Add(full)) continue;

                    var fileName = System.IO.Path.GetFileNameWithoutExtension(apPath);
                    var parentDir = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(apPath)) ?? "";
                    var display = string.IsNullOrEmpty(parentDir) ? fileName : $"{fileName}  ({parentDir})";

                    found.Add(new ProjectItem(display, apPath));
                }

                if (found.Count == 0)
                {
                    lblStatus1.Text = "No .ap* project files found under the specified folder.";
                    return;
                }

                foreach (var item in found.OrderBy(p => p.Name))
                    comboBoxTIAprojects.Items.Add(item);

                comboBoxTIAprojects.SelectedIndex = 0;
                _selectedProjectPath = (comboBoxTIAprojects.SelectedItem as ProjectItem).Path;
                lblStatus1.Text = $"Found {comboBoxTIAprojects.Items.Count} project(s).";
            }
            catch (UnauthorizedAccessException)
            {
                lblStatus1.Text = "Access denied when scanning the folder.";
            }
            catch (System.IO.PathTooLongException)
            {
                lblStatus1.Text = "A path encountered while scanning is too long.";
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error: " + ex.Message;
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void generateTemplate(object sender, EventArgs e)
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

        private void add_DB(object sender, EventArgs e)
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
    }
}
