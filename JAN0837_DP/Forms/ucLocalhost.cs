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
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Hosting.Server;
using static JAN0837_DP.Data.TestData;

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
            //txtBoxParam1.Text = "Text";
            //txtBoxParam2.Text = "0";
            //txtBoxParam3.Text = "false";

            if (webView21.CoreWebView2 == null)
            {
                await webView21.EnsureCoreWebView2Async();
                webView21.DefaultBackgroundColor = Color.WhiteSmoke;
            }

            _feCommunication = new FEcommunicationControl(internalVariables.communicationBaseURL);
            _feCommunication.communicationStart();

            await _feCommunication.EnsureCommunicationServiceAsync();  // port 5000
            await _feCommunication.EnsureReactDevServerAsync();        // port 3000

            webView21.CoreWebView2.Navigate(internalVariables.feURL);
            //lblCommunicationStatus.Text = "FE running (3000) & API ready (5000)";


            if (internalVariables.feServerStarted == true)
            {
                webView21.CoreWebView2.Navigate(internalVariables.feURL);
            }
        }

        private async void btnStartFE_Click(object sender, EventArgs e)
        {
            if (internalVariables.communicationServerStarted != true)
            {
                _feCommunication = new FEcommunicationControl(internalVariables.communicationBaseURL);
                _feCommunication.communicationStart();
            }

            if (internalVariables.feServerStarted != true)
            {
                try
                {
                    await _feCommunication.EnsureCommunicationServiceAsync();  // port 5000
                    await _feCommunication.EnsureReactDevServerAsync();        // port 3000

                    webView21.CoreWebView2.Navigate(internalVariables.feURL);
                    //lblCommunicationStatus.Text = "FE running (3000) & API ready (5000)";
                }
                catch (Exception ex)
                {

                }
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
        /*
        private void btnShowData_Click(object sender, EventArgs e)
        {
            var data = TestData.AppState.Get();

            lblData.Text =
                $"number     = {data.number}\r\n" +
                $"text       = {data.text}\r\n" +
                $"toggle     = {data.toggle}\r\n" +
                $"ToggleBool = {data.ToggleBool}";
        }
        */
    }
}
