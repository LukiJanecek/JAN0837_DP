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
            // run python script 
            lblStatus1.Text = "Starting python...";
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
