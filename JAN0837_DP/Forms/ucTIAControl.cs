using JAN0837_DP.Data;
using JAN0837_DP.TIA;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace JAN0837_DP.Forms
{
    public partial class ucTIAControl : UserControl
    {
        public TiaPortal tiaPortal;
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

            chBoxChangeTiaDLLPath.Enabled = true;
            chBoxChangeTiaDLLPath.Visible = true;
            chBoxChangeTiaDLLPath.Text = "Change DLL";

            // lbls
            lblParam1.Text = "";
            lblParam2.Text = "";
            lblParam3.Text = "";

            lblDLLpath.Text = "Path to Siemens.Engineering.dll:";
            lblTiaProject.Text = "Select TIA Portal project:";

            txtBoxTIADLL.Enabled = false;
            txtBoxTIADLL.Text = paths.defaultTIADLLPath;

            // Detect initial TIA version
            TIAcontrol.tiaPortalVersion = TIAcontrol.DetectTIAVersion(paths.defaultTIADLLPath);

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

            btnUsePresetDLLPath.Enabled = true;
            btnUsePresetDLLPath.Visible = true;

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

            btnChangeDLLPath.Enabled = true;
            btnChangeDLLPath.Visible = true;

            btnChangeProjectPath.Enabled = false;
            btnChangeProjectPath.Visible = false;

            // lbls
            lblTiaProject.Visible = false;
            lblParam1.Visible = false;
            lblParam2.Visible = false;
            lblParam3.Visible = false;

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
                var found = new List<TIAcontrol.ProjectItem>();

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

                            found.Add(new TIAcontrol.ProjectItem(display, apPath));
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
                    // Sort: matching version first, then by name
                    var sorted = TIAcontrol.tiaPortalVersion.HasValue
                        ? found.OrderByDescending(p => p.IsVersionMatch(TIAcontrol.tiaPortalVersion))
                              .ThenBy(p => p.Name)
                        : found.OrderBy(p => p.Name);

                    foreach (var item in sorted)
                        comboBoxTIAprojects.Items.Add(item);

                    comboBoxTIAprojects.SelectedIndex = 0;
                    TIAcontrol._selectedProjectPath = (comboBoxTIAprojects.SelectedItem as TIAcontrol.ProjectItem).Path;

                    int maxWidth = comboBoxTIAprojects.Width;

                    using (Graphics g = comboBoxTIAprojects.CreateGraphics())
                    {
                        foreach (var item in comboBoxTIAprojects.Items)
                        {
                            int itemWidth = (int)g.MeasureString(item.ToString(), comboBoxTIAprojects.Font).Width;
                            if (itemWidth > maxWidth)
                                maxWidth = itemWidth;
                        }
                    }

                    // Add some padding for comfort
                    comboBoxTIAprojects.DropDownWidth = maxWidth + 20;

                    // Build status message
                    var statusMsg = $"Found {comboBoxTIAprojects.Items.Count} project(s). ";
                    if (TIAcontrol.tiaPortalVersion.HasValue)
                    {
                        var matchingCount = found.Count(p => p.IsVersionMatch(TIAcontrol.tiaPortalVersion));
                        var mismatchCount = found.Count - matchingCount;

                        statusMsg += $"({matchingCount} matching V{TIAcontrol.tiaPortalVersion.Value}";
                        if (mismatchCount > 0)
                            statusMsg += $", {mismatchCount} different versions - may not open)";
                        else
                            statusMsg += ")";
                    }

                    lblStatus1.Text = statusMsg;
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
            lblStatus1.Text = "Choose your project.";

            // UI settings 
            #region UI settings

            // txtBoxes
            txtBoxParam1.Enabled = true;
            txtBoxParam1.Visible = true;
            txtBoxParam1.Text = paths.tiaProjectPath;

            txtBoxParam2.Enabled = false;
            txtBoxParam2.Visible = false;

            txtBoxParam3.Enabled = false;
            txtBoxParam3.Visible = false;

            // lbls
            lblParam1.Visible = true;
            lblParam2.Visible = false;
            lblParam3.Visible = false;

            lblParam1.Text = "Type your project path: ";

            // cbbox
            comboBoxTIAprojects.Enabled = true;
            comboBoxTIAprojects.Visible = true;

            lblTiaProject.Visible = true;
            lblTiaProject.Text = "TIA Portal projects from folder:";

            // btns
            btnPreset.Enabled = false;
            btnPreset.Visible = false;

            btnCreateProject.Enabled = false;
            btnCreateProject.Visible = false;

            btnOpenProject.Enabled = true;
            btnOpenProject.Visible = true;

            btnAddDB.Enabled = true;
            btnAddDB.Visible = true;

            btnFindTIAProjectOnPath.Enabled = true;
            btnFindTIAProjectOnPath.Visible = true;

            btnChangeProjectPath.Enabled = true;
            btnChangeProjectPath.Visible = true;

            #endregion

            FindMyProjectInThisProject();
        }

        private void rbtnCreateNewProject_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus1.Text = "Type your inputs and create new template project.";

            // UI settings 
            #region UI settings 

            // txtBoxes
            txtBoxParam1.Enabled = true;
            txtBoxParam1.Visible = true;
            txtBoxParam1.Text = "";

            txtBoxParam2.Enabled = true;
            txtBoxParam2.Visible = true;

            txtBoxParam3.Enabled = true;
            txtBoxParam3.Visible = true;

            txtBoxParam3.Enabled = true;
            txtBoxParam3.Visible = true;

            // lbls
            lblParam1.Visible = true;
            lblParam2.Visible = true;
            lblParam3.Visible = true;

            lblParam1.Text = "New project path:";
            lblParam2.Text = "Project name:";
            lblParam3.Text = "PLC name:";

            // cbBox
            comboBoxTIAprojects.Enabled = true;
            comboBoxTIAprojects.Visible = true;

            lblTiaProject.Visible = true;
            lblTiaProject.Text = "CPU type ID:";

            // btns 
            btnPreset.Enabled = true;
            btnPreset.Visible = true;

            btnCreateProject.Enabled = true;
            btnCreateProject.Visible = true;

            btnOpenProject.Enabled = false;
            btnOpenProject.Visible = false;

            btnAddDB.Enabled = false;
            btnAddDB.Visible = false;

            btnFindTIAProjectOnPath.Enabled = false;
            btnFindTIAProjectOnPath.Visible = false;

            btnChangeProjectPath.Enabled = true;
            btnChangeProjectPath.Visible = true;

            #endregion

            PopulateCPUTypeIDs();
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

        #endregion

        private void comboBoxTIAprojects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTIAprojects.SelectedItem is TIAcontrol.ProjectItem item)
            {
                TIAcontrol._selectedProjectPath = item.Path;
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

        private async void btnStartTIA_Click(object sender, EventArgs e)
        {
            lblStatus1.Text = "Starting TIA Portal...";

            //tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface); // python will do this 

            string[] args = new[] { "--dll-dir", paths.tiaDLLPath, "--ui" };
            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "startTIAPortal.py");

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

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

                    lblStatus1.Text = $"Starting TIA Portal failed. Check DLL path and project version.";

                    // Show detailed error
                    MessageBox.Show(msg.ToString(), "Error starting TIA Portal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Success
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    lblStatus1.Text = "TIA Portal started successfully: " + stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                else
                {
                    lblStatus1.Text = "TIA Portal started successfully.";
                }
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Exception error: " + ex.Message;
                MessageBox.Show($"Error: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnOpenProject_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TIAcontrol._selectedProjectPath))
            {
                lblStatus1.Text = "Please select a TIA project first.";
                return;
            }

            // Check version compatibility
            var selectedItem = comboBoxTIAprojects.SelectedItem as TIAcontrol.ProjectItem;
            if (selectedItem != null && !selectedItem.IsVersionMatch(TIAcontrol.tiaPortalVersion))
            {
                var projectVer = selectedItem.Version?.ToString() ?? "unknown";
                var dllVer = TIAcontrol.tiaPortalVersion?.ToString() ?? "unknown";

                var result = MessageBox.Show(
                    $"Version Mismatch Warning!\n\n" +
                    $"Project Version: V{projectVer}\n" +
                    $"DLL Version: V{dllVer}\n\n" +
                    $"Opening a project with a different TIA Portal version may fail or cause issues.\n\n" +
                    $"Do you want to continue anyway?",
                    "Version Mismatch",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                {
                    lblStatus1.Text = "Operation cancelled by user.";
                    return;
                }
            }

            lblStatus1.Text = "Opening TIA project...";

            string[] args = new[] { "--dll-dir", paths.tiaDLLPath, "--project-dir", TIAcontrol._selectedProjectPath, "--ui" };
            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "openPathProject.py");

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

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

                    lblStatus1.Text = $"Opening project failed. Check DLL path and project version.";

                    // Show detailed error
                    MessageBox.Show(msg.ToString(), "Error Opening Project", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Success
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    lblStatus1.Text = "Project opened successfully: " + stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                else
                {
                    lblStatus1.Text = "Project opened successfully.";
                }
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Exception error: " + ex.Message;
                MessageBox.Show($"Error: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnCreateProject_Click(object sender, EventArgs e)
        {
            string cputype;

            // Extract only the TypeId without the description
            if (comboBoxTIAprojects.SelectedItem is TIAcontrol.CPUTypeItem cpuItem)
            {
                cputype = cpuItem.TypeId;
            }
            else
            {
                lblStatus1.Text = "Please select a CPU type, please.";
                return;
            }

            string projectPath = txtBoxParam1.Text.Trim();

            if (string.IsNullOrWhiteSpace(projectPath))
            {
                lblStatus1.Text = "Type new project path, please.";
                return;
            }

            string projectName = txtBoxParam3.Text.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                lblStatus1.Text = "Type project name, please.";
                return;
            }

            string plcName = txtBoxParam3.Text.Trim();
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

            //string arg = $"--dir {projectPath} --name {projectName} --type-id {cputype} --plc-name {plcName} --ui";
            string[] args = new[] { "--dll-dir", paths.tiaDLLPath, "--project-dir", projectPath, "--project-name", projectName, "--type-id", cputype, "--plc-name", plcName, "--ui" };

            ////
            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "createNewTIAPortalProject.py");

            lblStatus1.Text = "Starting generating template...";

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

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
            if (string.IsNullOrWhiteSpace(TIAcontrol._selectedProjectPath))
            {
                lblStatus1.Text = "Please select a TIA project first.";
                throw new InvalidOperationException("Please select a TIA project first.");
            }

            string[] args = new[] { "--dll-dir", paths.tiaDLLPath, "--project-dir", TIAcontrol._selectedProjectPath };

            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "addDBtoPathProject.py");

            lblStatus1.Text = "Adding DB to project.";

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

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

            // Detect version from the path BEFORE importing
            string newDllPath = txtBoxTIADLL.Text.Trim();
            int? detectedVersion = TIAcontrol.DetectTIAVersion(newDllPath);

            // Show what we're trying to import
            if (detectedVersion.HasValue)
            {
                lblStatus1.Text = $"Importing TIA Portal V{detectedVersion.Value} DLL...";
            }
            else
            {
                lblStatus1.Text = "Importing DLL (version unknown from path)...";
            }

            // test import on current path in txtBoxTIADLL
            string[] args = new[] { "--dir", newDllPath };

            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "importTIADLL.py");

            try
            {
                var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, args);

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

                    // Show what version we tried to import (even though it failed)
                    if (detectedVersion.HasValue)
                    {
                        lblStatus1.Text = $"Import of TIA Portal V{detectedVersion.Value} failed. Check DLL path.";
                    }
                    else
                    {
                        lblStatus1.Text = $"Import failed. Check path to Siemens.Engineering.dll.";
                    }

                    // Show error details
                    MessageBox.Show(msg.ToString(), "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Success! Update the path and version
                paths.tiaDLLPath = newDllPath;
                TIAcontrol.tiaPortalVersion = detectedVersion;

                // Show success message with detected version
                if (detectedVersion.HasValue)
                {
                    lblStatus1.Text = $"Successfully imported TIA Portal V{detectedVersion.Value}";
                }
                else
                {
                    lblStatus1.Text = "Import successful (version not detected from path)";
                }

                // Refresh project list with new version
                if (rbtnOpenProject.Checked == true)
                {
                    FindMyProjectInThisProject();
                }
            }
            catch (Exception ex)
            {
                if (detectedVersion.HasValue)
                {
                    lblStatus1.Text = $"Exception importing V{detectedVersion.Value}: {ex.Message}";
                }
                else
                {
                    lblStatus1.Text = $"Exception during import: {ex.Message}";
                }

                MessageBox.Show($"Error: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPreset_Click(object sender, EventArgs e)
        {
            txtBoxParam1.Text = paths.tiaProjectPath;
            txtBoxParam2.Text = "MyAwesomeTIAPortalproject";
            txtBoxParam3.Text = "PLC_1";
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
                // Path must be an existing directory from here
                if (!System.IO.Directory.Exists(inputPath))
                {
                    lblStatus1.Text = "Directory does not exist: " + inputPath;
                    return;
                }

                // If user specified a file directly, accept it when it's a .ap* file
                if (System.IO.Path.GetExtension(inputPath).StartsWith(".ap", StringComparison.OrdinalIgnoreCase))
                {
                    var display = System.IO.Path.GetFileNameWithoutExtension(inputPath);
                    comboBoxTIAprojects.Items.Add(new TIAcontrol.ProjectItem(display, inputPath));
                    comboBoxTIAprojects.SelectedIndex = 0;
                    TIAcontrol._selectedProjectPath = inputPath;
                    lblStatus1.Text = $"Found {comboBoxTIAprojects.Items.Count} project file.";
                    return;
                }
                
                // Search recursively for any .ap* files under the given folder
                var apFiles = System.IO.Directory.EnumerateFiles(inputPath, "*.ap*", System.IO.SearchOption.AllDirectories);

                var found = new List<TIAcontrol.ProjectItem>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var apPath in apFiles)
                {
                    var full = System.IO.Path.GetFullPath(apPath);
                    if (!seen.Add(full)) continue;

                    var fileName = System.IO.Path.GetFileNameWithoutExtension(apPath);
                    var parentDir = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(apPath)) ?? "";
                    var display = string.IsNullOrEmpty(parentDir) ? fileName : $"{fileName}  ({parentDir})";

                    found.Add(new TIAcontrol.ProjectItem(display, apPath));
                }

                if (found.Count == 0)
                {
                    lblStatus1.Text = "No .ap* project files found under the specified folder.";
                    return;
                }

                foreach (var item in found.OrderBy(p => p.Name))
                    comboBoxTIAprojects.Items.Add(item);

                comboBoxTIAprojects.SelectedIndex = 0;
                TIAcontrol._selectedProjectPath = (comboBoxTIAprojects.SelectedItem as TIAcontrol.ProjectItem).Path;
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

        private void btnChangeDLLPath_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Choose your folder with Siemens.Engineering.dll.";
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBoxTIADLL.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnUsePresetDLLPath_Click(object sender, EventArgs e)
        {
            txtBoxTIADLL.Text = paths.defaultTIADLLPath;
        }

        private void btnChangeProjectPath_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Choose your folder with TIA Portal project .ap*.";
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBoxParam1.Text = dialog.SelectedPath;
                }
            }
        }

        private void PopulateCPUTypeIDs()
        {
            comboBoxTIAprojects.Items.Clear();

            // Add default CPU type IDs with descriptions
            var cpuTypes = new List<TIAcontrol.CPUTypeItem>
            {
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 212-1AE40-0XB0/V4.6", "S7-1200 CPU 1212C DC/DC/DC"),
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 214-1AG40-0XB0/V4.6", "S7-1200 CPU 1214C DC/DC/DC"),
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 215-1AG40-0XB0/V4.6", "S7-1200 CPU 1215C DC/DC/DC"),
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 217-1AG40-0XB0/V4.6", "S7-1200 CPU 1217C DC/DC/DC"),
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 511-1AK02-0AB0/V2.0", "S7-1500 CPU 1511-1 PN"),
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 513-1AL02-0AB0/V2.0", "S7-1500 CPU 1513-1 PN"),
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 515-2AM02-0AB0/V2.0", "S7-1500 CPU 1515-2 PN"),
                new TIAcontrol.CPUTypeItem("OrderNumber:6ES7 517-3AP00-0AB0/V2.0", "S7-1500 CPU 1517-3 PN/DP"),
            };

            foreach (var cpuType in cpuTypes)
            {
                comboBoxTIAprojects.Items.Add(cpuType);
            }

            comboBoxTIAprojects.SelectedIndex = 0;

            int maxWidth = comboBoxTIAprojects.Width;

            using (Graphics g = comboBoxTIAprojects.CreateGraphics())
            {
                foreach (var item in comboBoxTIAprojects.Items)
                {
                    int itemWidth = (int)g.MeasureString(item.ToString(), comboBoxTIAprojects.Font).Width;
                    if (itemWidth > maxWidth)
                        maxWidth = itemWidth;
                }
            }
            // Add some padding for comfort
            comboBoxTIAprojects.DropDownWidth = maxWidth + 20;

            lblStatus1.Text = "Select CPU type in new project.";
        }
    }
}
