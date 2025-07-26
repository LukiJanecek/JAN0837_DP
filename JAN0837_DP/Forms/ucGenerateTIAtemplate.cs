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
// Openness povolený (v TIA Portal: Settings → General → Engineering → Enable Openness API)
// někde bude Siemens.Engineering.dll a poté je třeba přidat referenci 
//
/*
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Blocks.Interface;
using System.ComponentModel.DataAnnotations;
*/

namespace JAN0837_DP.Forms
{
    public partial class ucGenerateTIAtemplate : UserControl
    {
        public ucGenerateTIAtemplate()
        {
            InitializeComponent();
        }

        private void ucGenerateTIAtemplate_Load(object sender, EventArgs e)
        {

        }

        private void btnGenerateTemplate_Click(object sender, EventArgs e)
        {
            /*
            TiaPortal tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);  // nebo WithoutUserInterface
            Project project = tiaPortal.Projects.Create(@"C:\TIA\MyProject", "MyProject");

            Device device = project.Devices.Create("CPU_1212C_DC_DC_DC", "V2.0"); // typ zařízení
            PlcSoftware plcSoftware = device.DeviceItems[1].GetService<SoftwareContainer>().Software as PlcSoftware;

            // Vytvoříme datový blok
            PlcBlockUserGroup group = plcSoftware.BlockGroup;
            PlcBlock db = group.Blocks.Create(PlcBlockType.DataBlock, "MyDB", PlcProgrammingLanguage.LAD);

            // Přidáme proměnnou
            var staticSection = db.Interface.Static;
            staticSection.Create("myInt", DataType.Int);
            staticSection.Create("myReal", DataType.Real);

            project.Save();
            tiaPortal.Dispose();
            */
        }
    }
}
