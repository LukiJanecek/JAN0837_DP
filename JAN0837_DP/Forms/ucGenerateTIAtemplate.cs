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

        private void btnGenerateTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtParam1.Text != "" || txtParam1.Text != null)
                {
                    lblStatus1.Text = "Generating template in TIA.";
                    
                    string tiaProjectFolder = Path.Combine(paths.tiaPath, txtParam1.Text);

                    Directory.CreateDirectory(tiaProjectFolder);
                    var projectFolderInfo = new DirectoryInfo(tiaProjectFolder);

                    lblStatus1.Text = "Running TIA Portal.";

                    tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);

                    lblStatus1.Text = "Creating folder with project.";

                    Project project = tiaPortal.Projects.Create(projectFolderInfo, txtParam1.Text);
                    var devices = project.Devices;
                    //object deviceItemRef;

                    lblStatus1.Text = "Adding PLC.";
                    var device = devices.CreateWithItem("CPU_1212C_DC_DC_DC", "V4.5", txtParam1.Text);
                    
                    // CPU module
                    DeviceItem plcDeviceItem = device.DeviceItems[0];

                    // PLC software
                    var softwareContainer = plcDeviceItem.GetService<SoftwareContainer>();
                    var plcSoftware = softwareContainer.Software as PlcSoftware;

                    // Data block 
                    lblStatus1.Text = "Adding FB + DB.";
                    var blockGroup = plcSoftware.BlockGroup;
                    var fb = blockGroup.Blocks.CreateFB("FB_test", true, 1,ProgrammingLanguage.LAD);
                    var db = blockGroup.Blocks.CreateInstanceDB("test", true, 1, "FB_test");

                    project.Save();
                    tiaPortal.Dispose();

                    lblStatus1.Text = "Template generated.";
                }
                else
                {
                    lblStatus1.Text = "Type project name to Parameter1.";
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                
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
