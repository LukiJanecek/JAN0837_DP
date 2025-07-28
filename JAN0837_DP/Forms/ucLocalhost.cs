using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;


using JAN0837_DP.Data;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.Web.WebView2.Core;
using Org.BouncyCastle.Asn1.Cmp;
using JAN0837_DP.ReactFE;

namespace JAN0837_DP.Forms
{
    public partial class ucLocalhost : UserControl
    {
        Process serverProcess = null;

        private ReactFE.FEcommunicationControl FE;

        public ucLocalhost()
        {
            InitializeComponent(); ;
        }

        private void ucLocalhost_Load(object sender, EventArgs e)
        {
            txtBoxParam1.Text = "Parameter1";
            txtBoxParam2.Text = "2000";
            txtBoxParam3.Text = "Empty";
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void btnStartFE_Click(object sender, EventArgs e)
        {

            string reactFolder = Path.Combine(MainForm.projectRootPath, "ReactFE");
            string reactPath = Path.Combine(reactFolder, "jan0837_reactfe");

            FE = new FEcommunicationControl(internalVariables.communicationURL);
            FE.Start();

            Process.Start(new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "start",
                WorkingDirectory = reactPath,
                UseShellExecute = true,
                CreateNoWindow = true
            });
            

            webView21.CoreWebView2.Navigate(internalVariables.feURL);
        }

        private async void btnSendDatatoFE_Click(object sender, EventArgs e)
        {
            FE.Update("status", "připojeno");
            FE.Update("parameter1", txtBoxParam1.Text);
            FE.Update("refreshInterval", int.Parse(txtBoxParam2.Text));

            var payload = new
            {
                refreshInterval = int.Parse(txtBoxParam2.Text)
            };
            string json = JsonConvert.SerializeObject(payload);

            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(internalVariables.communicationURL + "config", content);
            response.EnsureSuccessStatusCode();
            lblCommunicationStatus.Text = "Data Trasnfered";
        }
    }
}
