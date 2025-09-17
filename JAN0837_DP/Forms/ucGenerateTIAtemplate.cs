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
using JAN0837_DP.TIA;

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
            lblStatus1.Text = "Set project name and cpu type id.";
            lblParam1.Text = "Project name: ";
            lblParam2.Text = "CPU type ID: ";
        }

        private async void btnGenerateTemplate_Click(object sender, EventArgs e)
        {
            // run python script 
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

            string pythonPath = Path.Combine(paths.pythonScriptsFolder, "");
            string pythonScriptPath = Path.Combine(paths.pythonScriptsFolder, "createTIAtemplate.py");

            var (code, so, se) = await tia.runPY(pythonPath, pythonScriptPath, args);

            if (code == 0)
            {
                lblStatus1.Text = "Template generated.";
            }
            else
            {
                lblStatus1.Text = "";
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
    }
}
