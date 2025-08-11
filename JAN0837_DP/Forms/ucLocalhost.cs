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
using static System.Net.WebRequestMethods;

namespace JAN0837_DP.Forms
{
    public partial class ucLocalhost : UserControl
    {
        Process serverProcess = null;

        private ReactFE.FEcommunicationControl FE;

        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(1500) };

        private Process reactDevServerProc;

        public ucLocalhost()
        {
            InitializeComponent();
        }

        private async void ucLocalhost_Load(object sender, EventArgs e)
        {
            txtBoxParam1.Text = "Text";
            txtBoxParam2.Text = "0";
            txtBoxParam3.Text = "false";

            if (webView21.CoreWebView2 == null)
            {
                await webView21.EnsureCoreWebView2Async();
            }

            if (internalVariables.reactServerStarted == true)
            {
                webView21.CoreWebView2.Navigate(internalVariables.feURL);
            }
        }

        private async Task<bool> IsAliveAsync(string url)
        {
            try
            {
                using var resp = await http.GetAsync(url);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        private async Task WaitUntilAliveAsync(string url, int timeoutMs = 30000, int pollMs = 500)
        {
            var start = Environment.TickCount;
            while (Environment.TickCount - start < timeoutMs)
            {
                if (await IsAliveAsync(url)) return;
                await Task.Delay(pollMs);
            }
            throw new TimeoutException($"Service not reachable: {url}");
        }

        private async Task EnsureCommunicationServiceAsync()
        {
            // API health-check: /api/status (nebo /api/data)
            var apiHealth = internalVariables.communicationURL + "status";

            if (!await IsAliveAsync(apiHealth))
            {
                // nespouštěj znovu, pokud FE už existuje a běží
                if (FE == null) FE = new FEcommunicationControl(internalVariables.communicationURL);
                FE.Start();

                await WaitUntilAliveAsync(apiHealth, timeoutMs: 5000);
            }
        }

        private async Task EnsureReactDevServerAsync()
        {
            // React dev server běží na http://localhost:3000/
            var feRoot = internalVariables.feURL; // "http://localhost:3000/"

            if (!await IsAliveAsync(feRoot))
            {
                // Pokud ne, spust ho
                string reactFolder = Path.Combine(MainForm.projectRootPath, "ReactFE");
                string reactPath = Path.Combine(reactFolder, "jan0837_reactfe");

                // Pokud proces už máme a žije, nespouštěj znovu
                if (reactDevServerProc == null || reactDevServerProc.HasExited)
                {
                    reactDevServerProc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "start",
                        WorkingDirectory = reactPath,
                        UseShellExecute = true,
                        CreateNoWindow = true
                    });
                }

                // Počkej, než dev server naběhne
                await WaitUntilAliveAsync(feRoot, timeoutMs: 60000);
            }
        }

        private async void btnStartFE_Click(object sender, EventArgs e)
        {

            FE = new FEcommunicationControl(internalVariables.communicationURL);
            FE.Start();
            /*
            Process.Start(new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "start",
                WorkingDirectory = paths.feReactProjectPath,
                UseShellExecute = true//,
                //CreateNoWindow = true
            });
            */
            

            try
            {
                await EnsureCommunicationServiceAsync();  // port 5000
                await EnsureReactDevServerAsync();        // port 3000

                webView21.CoreWebView2.Navigate(internalVariables.feURL);
                lblCommunicationStatus.Text = "FE running (3000) & API ready (5000)";
            }
            catch (Exception ex)
            {

            }
        }

        private void txtBoxParam1_TextChanged(object sender, EventArgs e)
        {
            if (txtBoxParam1.Text != null && txtBoxParam1.Text != "")
            {
                TestData.text = txtBoxParam1.Text;
            }
        }

        private void txtBoxParam2_TextChanged(object sender, EventArgs e)
        {
            if (txtBoxParam2.Text != null && txtBoxParam2.Text != "")
            {
                TestData.number = int.Parse(txtBoxParam2.Text);
            }
        }

        private void txtBoxParam3_TextChanged(object sender, EventArgs e)
        {
            if (txtBoxParam3.Text != null && txtBoxParam3.Text != "")
            {
                TestData.toggle = txtBoxParam3.Text;
            }
        }

        private async void btnSendDataToFe_Click(object sender, EventArgs e)
        {
            // připrav model
            var data = new Dictionary<string, object>
            {
                ["number"] = TestData.number,
                ["text"] = TestData.text,
                ["toggle"] = TestData.toggle
            };

            // pošli na /api/data
            string json = JsonConvert.SerializeObject(data);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.PostAsync(internalVariables.communicationURL + "data", content);
            response.EnsureSuccessStatusCode();

            lblCommunicationStatus.Text = "Data transferred";
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnShowPage_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.Navigate(internalVariables.feURL);
        }
    }
}
