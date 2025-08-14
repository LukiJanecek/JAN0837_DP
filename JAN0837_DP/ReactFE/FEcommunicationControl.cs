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
using Newtonsoft.Json;

namespace JAN0837_DP.ReactFE
{
    public class FEcommunicationControl
    {
        private readonly string _prefix;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public FEcommunicationControl(string prefix)
        {
            var basePrefix = string.IsNullOrWhiteSpace(prefix) ? internalVariables.communicationBaseURL : prefix;

            _prefix = (basePrefix ?? "http://localhost:5000/api/").TrimEnd('/') + "/";

            // var prefix = (internalVariables.communicationBaseURL ?? "http://localhost:5000/api/").TrimEnd('/') + "/";
        }

        public void Start()
        {
            if (_listener?.IsListening == true) 
            {
                return;
            }

            if (internalVariables.communicationServerStarted == true)
            {
                return;
            }
            else
            {
                _listener = new HttpListener();
                _listener.Prefixes.Clear();
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
                try
                {
                    _cts?.Cancel();
                    _listener?.Stop();
                }
                finally
                {
                    _listener = null;
                    internalVariables.communicationServerStarted = false; 
                }


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

        private void AddCors(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var allowedOrigin = (internalVariables.feURL ?? "http://localhost:3000").TrimEnd('/');
            var origin = (req.Headers["Origin"] ?? "").TrimEnd('/');

            if (!string.IsNullOrEmpty(origin) && origin.Equals(allowedOrigin, StringComparison.OrdinalIgnoreCase))
            {
                resp.Headers["Access-Control-Allow-Origin"] = origin;
            }
            else
            {
                resp.Headers["Access-Control-Allow-Origin"] = allowedOrigin;
            }

            resp.Headers["Vary"] = "Origin";
            resp.Headers["Access-Control-Allow-Methods"] = "GET,POST,OPTIONS";
            resp.Headers["Access-Control-Allow-Headers"] = "Content-Type";
            resp.Headers["Access-Control-Max-Age"] = "600";
        }

        private void Process(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;

            AddCors(req, resp);

            // Preflight pro CORS
            if (req.HttpMethod == "OPTIONS")
            {
                resp.StatusCode = 204; // No Content
                resp.Close();
                return;
            }

            var path = req.Url.AbsolutePath.ToLowerInvariant().TrimEnd('/');
            const string apiPrefix = "/api";

            if (path.StartsWith(apiPrefix))
            {
                path = path.Substring(4);
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            try
            {
                if (req.HttpMethod == "GET" && (path == "/data" || path == "/"))
                {
                    // celý stav
                    WriteJSON(resp, new
                    {
                        number = TestData.number,
                        text = TestData.text,
                        toggle = TestData.toggle
                    });
                    return;
                }
                else if (req.HttpMethod == "POST" && (path == "/data" || path == "/"))
                {
                    using var sr = new StreamReader(req.InputStream);
                    var body = sr.ReadToEnd();

                    var updates = JsonConvert.DeserializeObject<Dictionary<string, object>>(body) ?? new Dictionary<string, object>();

                    if (updates.TryGetValue("number", out var n))
                    {
                        TestData.number = Convert.ToInt32(n);
                    }

                    if (updates.TryGetValue("text", out var t))
                    {
                        TestData.text = Convert.ToString(t);
                    }

                    if (updates.TryGetValue("toggle", out var g))
                    {
                        TestData.toggle = Convert.ToString(g);
                    }

                    resp.StatusCode = 200;
                    resp.Close();
                    return;
                }
                else
                {
                    resp.StatusCode = 405; resp.Close(); return;
                }
            }
            catch (Exception ex)
            {
                // jednoduchý JSON error (ať to líp debuguješ v Network panelu)
                var payload = Encoding.UTF8.GetBytes($"{{\"error\":\"{ex.Message}\"}}");
                resp.StatusCode = 500;
                resp.ContentType = "application/json";
                resp.ContentLength64 = payload.Length;
                resp.OutputStream.Write(payload, 0, payload.Length);
                resp.Close();
            }

            /*
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
            */
        }

        private void WriteJSON(HttpListenerResponse resp, object data)
        {
            var json = JsonConvert.SerializeObject(data);   
            var buf = Encoding.UTF8.GetBytes(json);
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
