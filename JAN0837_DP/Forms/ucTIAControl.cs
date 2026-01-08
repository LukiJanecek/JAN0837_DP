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

            lblDLLpath.Text = "Path to DLL project: ";
            lblTiaProject.Text = "Select TIA project: ";

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

            // comboBox
            comboBoxTIAprojects.Items.Clear();

            btnFindTIAProjectOnPath_Click(sender, e);
        }

        // radio buttons
        #region
        private void rbtnOpenProject_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus1.Text = "Choose your project.";

            // UI settings 
            #region

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

        }

        private void rbtnCreateNewProject_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus1.Text = "Type your inputs and create new template project.";

            // UI settings 
            #region

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

        private void btnOpenProject_Click(object sender, EventArgs e)
        {
            // this doesnt work properly -> python will do this :) 
            try
            {
                lblStatus1.Text = "Openning TIA project.";

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
                lblStatus1.Text = "Error: " + ex.Message;
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

            lblStatus1.Text = "Starting generating template...";

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

            string args = $"--dir {projectPath} --name {projectName} --type-id {cputype} --ui";

            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "createTIAtemplate.py");

            var (code, stdout, stderr) = await TIAcontrol.runPY(paths.pythonExePath, pythonScriptPath, "--dir", projectPath, "--name", projectName, "--type-id", cputype, "--ui");

            if (code == 0)
            {
                lblStatus1.Text = "Template generated.";
            }
            else
            {
                lblStatus1.Text = "Generating failed.";
            }
        }

        private void btnAddDB_Click(object sender, EventArgs e)
        {

        }

        private void btnImportDLL_Click(object sender, EventArgs e)
        {
            // test import on current paths.tiaDLLPath
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
            // correctly it should be looking in parent folder (paths.tiaProjectPath) for Sample and Example projects
            comboBoxTIAprojects.Items.Clear();

            try
            {
                var candidates = new[]
                {
                paths.tiaExampleProjectPath, paths.tiaSampleProjectPath
                };

                // find created .ap* projects 
                foreach (var baseDir in candidates.Where(Directory.Exists))
                {
                    // looking for .ap* file id directory, if not found look one level deeper
                    foreach (var projectDir in Directory.EnumerateDirectories(baseDir))
                    {
                        var apFilesTop = Directory.EnumerateFiles(projectDir, "*.ap*", SearchOption.TopDirectoryOnly);
                        var apPath = apFilesTop.FirstOrDefault();

                        if (apPath == null)
                        {
                            // some template projects has subdirectory with real .ap file 
                            var apFilesDeep = Directory.EnumerateFiles(projectDir, "*.ap*", SearchOption.AllDirectories);
                            apPath = apFilesDeep.OrderBy(p => p.Count(c => c == System.IO.Path.DirectorySeparatorChar)).FirstOrDefault();
                        }
                        else
                        {
                            var display = $"{System.IO.Path.GetFileNameWithoutExtension(apPath)}  ({System.IO.Path.GetFileName(projectDir)})";
                            comboBoxTIAprojects.Items.Add(new ProjectItem(display, apPath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus1.Text = "Error: " + ex.Message;
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
