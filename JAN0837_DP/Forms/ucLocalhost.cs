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
using JAN0837_DP.ReactFE;
using static System.Net.WebRequestMethods;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Hosting.Server;
using static JAN0837_DP.Data.TestData;
using JAN0837_DP.Log;

namespace JAN0837_DP.Forms
{
    public partial class ucLocalhost : UserControl
    {
        Process serverProcess = null;

        private System.Windows.Forms.Timer _pollTimer;

        public FEserver _feServer;
        public FEcommunicationControl _feCommunication;

        public ucLocalhost()
        {
            InitializeComponent();
        }

        private async void ucLocalhost_Load(object sender, EventArgs e)
        {
            try
            {
                if (webView21.CoreWebView2 == null)
                {
                    await webView21.EnsureCoreWebView2Async();
                    webView21.DefaultBackgroundColor = Color.WhiteSmoke;
                }

                if (internalVariables.communicationServerStarted == false)
                {
                    _feCommunication = new FEcommunicationControl(internalVariables.communicationBaseURL);
                    _feCommunication.communicationStart();
                }

                if (internalVariables.feServerStarted == false)
                {
                    _feCommunication ??= new FEcommunicationControl(internalVariables.communicationBaseURL);
                    await _feCommunication.EnsureCommunicationServiceAsync();  // port 5000
                    await _feCommunication.EnsureReactDevServerAsync();        // port 3000

                    webView21.CoreWebView2.Navigate(internalVariables.feURL);
                }
                else
                {
                    webView21.CoreWebView2.Navigate(internalVariables.feURL);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "ucLocalhost_Load");
                MessageBox.Show(
                    "Vizualizační server se nepodařilo spustit.\n\n" +
                    ex.Message +
                    "\n\nSpusťte deploy.ps1 jako správce. Pokud problém trvá, " +
                    "zkontrolujte, zda porty 3000 nebo 5000 nepoužívá jiná aplikace.",
                    "JAN0837_DP – chyba serveru",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private async void btnStartFE_Click(object sender, EventArgs e)
        {
            try
            {
                if (internalVariables.communicationServerStarted != true)
                {
                    _feCommunication = new FEcommunicationControl(internalVariables.communicationBaseURL);
                    _feCommunication.communicationStart();
                }

                if (internalVariables.feServerStarted != true)
                {
                    _feCommunication ??= new FEcommunicationControl(internalVariables.communicationBaseURL);
                    await _feCommunication.EnsureCommunicationServiceAsync();  // port 5000
                    await _feCommunication.EnsureReactDevServerAsync();        // port 3000

                    webView21.CoreWebView2.Navigate(internalVariables.feURL);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error starting FE or communication service");
                MessageBox.Show(
                    ex.Message,
                    "JAN0837_DP – chyba serveru",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        /*
        private void txtBoxParam1_TextChanged(object sender, EventArgs e)
        {
            if (txtBoxParam1.Text != null && txtBoxParam1.Text != "")
            {
                var data = TestData.AppState.Get();
                data.text = txtBoxParam1.Text ?? "";
                TestData.AppState.Set(data);
            }
        }

        private void txtBoxParam2_TextChanged(object sender, EventArgs e)
        {
            if (txtBoxParam2.Text != null && txtBoxParam2.Text != "")
            {
                if (int.TryParse(txtBoxParam2.Text, out var number))
                {
                    var data = TestData.AppState.Get();
                    data.number = number;
                    TestData.AppState.Set(data);
                }
            }
        }

        private void txtBoxParam3_TextChanged(object sender, EventArgs e)
        {
            if (txtBoxParam3.Text != null && txtBoxParam3.Text != "")
            {
                var data = TestData.AppState.Get();
                data.toggle = txtBoxParam3.Text ?? "";
                TestData.AppState.Set(data);
            }
        }
        */

        /*
        private async void btnSendDataToFe_Click(object sender, EventArgs e)
        {
            var data = TestData.AppState.Get();

            var payload = new Dictionary<string, object>
            {
                ["number"] = data.number,
                ["text"] = data.text,
                ["toggle"] = data.toggle
            };

            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            string json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(internalVariables.communicationDataURL, content);
            response.EnsureSuccessStatusCode();

            lblCommunicationStatus.Text = "Data transferred";
        }
        */

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


        /*
        private async void btnGetData_Click(object sender, EventArgs e)
        {
            try
            {
                btnGetData.Enabled = false; // proti 2x kliknutí
                lblCommunicationStatus.Text = "Fetching...";

                var snap = await _feCommunication.GetDataAsync();

                lblCommunicationStatus.Text = "Get data OK";
            }
            catch (Exception ex)
            {
                lblCommunicationStatus.Text = "Get data failed";
                lblData.Text = ex.Message;
            }
            finally
            {
                btnGetData.Enabled = true;
            }
        }
        */

        private void btnOpenDevTool_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.OpenDevToolsWindow();
        }

        private async void btnStopFE_Click(object sender, EventArgs e)
        {
            if (_feServer != null)
            {
                await _feServer.serverStop();
            }

            if (_feCommunication != null)
            {
                _feCommunication.communicationStop();
            }
        }
    }
}
