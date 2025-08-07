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
            txtBoxParam1.Text = "Text";
            txtBoxParam2.Text = "Number";
            txtBoxParam3.Text = "Boolean state";
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void btnStartFE_Click(object sender, EventArgs e)
        {

            FE = new FEcommunicationControl(internalVariables.communicationURL);
            FE.Start();

            Process.Start(new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "start",
                WorkingDirectory = paths.feReactProjectPath,
                UseShellExecute = true//,
                //CreateNoWindow = true
            });

            webView21.CoreWebView2.Navigate(internalVariables.feURL);
        }

        private void txtBoxParam1_TextChanged(object sender, EventArgs e)
        {
            TestData.text = txtBoxParam1.Text;
        }

        private void txtBoxParam2_TextChanged(object sender, EventArgs e)
        {
            TestData.number = int.Parse(txtBoxParam2.Text);
        }

        private void txtBoxParam3_TextChanged(object sender, EventArgs e)
        {
            TestData.toggle = Boolean.Parse(txtBoxParam3.Text);
        }
    }
}
