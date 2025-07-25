using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JAN0837_DP.Data;
using Microsoft.Web.WebView2.Core;

namespace JAN0837_DP.Forms
{
    public partial class ucLocalhost : UserControl
    {
        public ucLocalhost()
        {
            InitializeComponent();;
        }

        private void ucLocalhost_Load(object sender, EventArgs e)
        {
            webView21.CoreWebView2.Navigate(internalVariables.localhosturl);
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }
    }
}
