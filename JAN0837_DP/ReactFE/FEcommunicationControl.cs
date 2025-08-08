using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;

using Microsoft.AspNet.SignalR;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using JAN0837_DP.Data;

namespace JAN0837_DP.ReactFE
{
    public class FEcommunicationControl
    {
        private readonly string _prefix;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public FEcommunicationControl(string prefix)
        {
            _prefix = prefix;
        }

        public void Start()
        {
            if (internalVariables.communicationServerStarted == true)
            {
                return;
            }
            else
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(_prefix);
                _listener.Start();

                _cts = new CancellationTokenSource();
                Task.Run(() => HandleAsync(_cts.Token));

                internalVariables.communicationServerStarted = true;
            }
        }

        public void Stop()
        {
            if (internalVariables.communicationServerStarted == true)
            {
                _cts.Cancel();
                _listener.Stop();
            }
            else
            {
                return;
            }
        }

        public void Update(string key, object value)
        {
            switch (key)
            {
                case "status":
                    TestData.text = Convert.ToString(value);
                    break;
                case "parameter1":
                    TestData.number = Convert.ToInt32(value); 
                    break;
                case "refreshInterval":
                    internalVariables.communicationRefreshInterval = Convert.ToInt32(value);
                    break;
            }
        }

        private async Task HandleAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var ctx = await _listener.GetContextAsync();
                Process(ctx);
            }
        }

        private void Process(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;
            var path = req.Url.AbsolutePath.ToLowerInvariant(); // e.g. "/api/status"

            // Odstraňeme prefix "/api"
            const string apiPrefix = "/api";
            if (path.StartsWith(apiPrefix))
                path = path.Substring(apiPrefix.Length);

            if (req.HttpMethod == "GET")
            {
                switch (path)
                {
                    case "/status":
                        WriteJSON(resp, TestData.text);
                        break;

                    case "/parameter1":
                        WriteJSON(resp, TestData.number);
                        break;

                    case "/config":
                        WriteJSON(resp, internalVariables.communicationRefreshInterval);
                        break;

                    default:
                        resp.StatusCode = 404;
                        resp.Close();
                        break;
                }
            }
            else if (req.HttpMethod == "POST")
            {
                using var ms = new MemoryStream();
                req.InputStream.CopyTo(ms);
                var body = Encoding.UTF8.GetString(ms.ToArray());

                bool valid = true;

                switch (path)
                {
                    case "/status":
                        // Tady se můžeš rozhodnout, zda držet string nebo boolean
                        TestData.text = JsonSerializer.Deserialize<string>(body);
                        break;

                    case "/parameter1":
                        TestData.number = JsonSerializer.Deserialize<int>(body);
                        break;

                    case "/config":
                        var doc = JsonSerializer.Deserialize<JsonElement>(body);
                        if (doc.TryGetProperty("refreshInterval", out var jv))
                            internalVariables.communicationRefreshInterval = jv.GetInt32();
                        else
                            valid = false;
                        break;

                    default:
                        valid = false;
                        break;
                }

                resp.StatusCode = valid ? 200 : 400;
                resp.Close();
            }
            else
            {
                resp.StatusCode = 405;
            }
        }

        private void WriteJSON(HttpListenerResponse resp, object data)
        {
            var buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = buf.Length;
            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.Close();
        }

        // Vrátí aktuální stav jako anonymní objekt.
        public object GetCurrentState()
        => new
        {
            TestData.text,
            TestData.number,
            internalVariables.communicationRefreshInterval
        };

        // Aplikuje příchozí slovníkové změny.
        public void HandleUpdate(Dictionary<string, object> updates)
        {
            foreach (var kv in updates)
                Update(kv.Key, kv.Value);
        }
    }
}
