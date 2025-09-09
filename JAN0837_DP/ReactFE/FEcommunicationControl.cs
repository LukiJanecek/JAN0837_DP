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
using System.Diagnostics;

namespace JAN0837_DP.ReactFE
{
    public class FEcommunicationControl
    {
        private readonly string _prefix;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public Process reactDevServerProc;

        public static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(1500) };

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
            var data = TestData.AppState.Get();

            switch (key)
            {
                case "status":
                    data.text = Convert.ToString(value) ?? "";
                    break;

                case "parameter1":
                    if (value is int i)
                    {
                        data.number = i;
                    }
                    else if (value is long l)
                    {
                        data.number = (int)l;
                    }
                    else if (int.TryParse(Convert.ToString(value), out var n))
                    {
                        data.number = n;
                    }
                    break;

                case "refreshInterval":
                    if (value is int ri)
                    {
                        internalVariables.communicationRefreshInterval = ri;
                    }
                    else if (value is long rl)
                    {
                        internalVariables.communicationRefreshInterval = (int)rl;
                    }
                    else if (int.TryParse(Convert.ToString(value), out var r))
                    {
                        internalVariables.communicationRefreshInterval = r;
                    }
                    break;
            }

            TestData.AppState.Set(data);
        }

        public async Task HandleAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var ctx = await _listener.GetContextAsync();
                HandleRequest(ctx);
            }
        }

        public void AddCors(HttpListenerRequest req, HttpListenerResponse resp)
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

        public void HandleRequest(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;

            AddCors(req, resp);

            // Preflight for CORS
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
                    var data = TestData.AppState.Get();

                    WriteJSON(resp, new
                    {
                        number = data.number,
                        text = data.text,
                        toggle = data.toggle
                    });
                    return;
                }
                else if (req.HttpMethod == "POST" && (path == "/data" || path == "/"))
                {
                    using var sr = new StreamReader(req.InputStream);
                    var body = sr.ReadToEnd();

                    var updates = JsonConvert.DeserializeObject<Dictionary<string, string>>(body) ?? new Dictionary<string, string>();

                    var data = TestData.AppState.Get();

                    if (updates.TryGetValue("number", out var ns) && int.TryParse(ns, out var n))
                    {
                        data.number = Convert.ToInt32(n);
                    }

                    if (updates.TryGetValue("text", out var t))
                    {
                        data.text = Convert.ToString(t);
                    }

                    if (updates.TryGetValue("toggle", out var g))
                    {
                        data.toggle = Convert.ToString(g);
                    }

                    TestData.AppState.Set(data);

                    resp.StatusCode = 200;
                    resp.Close();
                    return;
                }
                else
                {
                    resp.StatusCode = 405; 
                    resp.Close(); 
                    return;
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
        }

        public void WriteJSON(HttpListenerResponse resp, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buf = Encoding.UTF8.GetBytes(json);
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = buf.Length;
            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.Close();
        }

        public object GetCurrentState()
        {
            var data = TestData.AppState.Get();
            return new
            {
                text = data.text,
                number = data.number,
                toggle = data.toggle
            };
        }

        public void HandleUpdate(Dictionary<string, object> updates)
        {
            foreach (var kv in updates)
                Update(kv.Key, kv.Value);
        }

        public async Task<TestData> GetDataAsync()
        {
            var url = internalVariables.communicationDataURL;
            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TestData>(json) ?? new TestData();
        }

        public void ApplySnapshot(dynamic snap)
        {
            //TestData.Update(snap.number, snap.text, snap.ToggleBool);

            TestData.AppState.Set(snap);

            Update("number", snap.number);
            Update("text", snap.text);
            Update("toggle", snap.toggle);
        }

        public async Task<bool> IsAliveAsync(string url)
        {
            try
            {
                using var resp = await http.GetAsync(url);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task WaitUntilAliveAsync(string url, int timeoutMs = 30000, int pollMs = 500)
        {
            var start = Environment.TickCount;
            while (Environment.TickCount - start < timeoutMs)
            {
                if (await IsAliveAsync(url)) return;
                await Task.Delay(pollMs);
            }
            throw new TimeoutException($"Service not reachable: {url}");
        }

        public async Task EnsureCommunicationServiceAsync()
        {
            // API health-check: /api/status (nebo /api/data)
            var apiHealth = internalVariables.communicationDataURL;

            if (!await IsAliveAsync(apiHealth))
            {
                await WaitUntilAliveAsync(apiHealth, timeoutMs: 5000);
            }
        }

        public async Task EnsureReactDevServerAsync()
        {
            if (!await IsAliveAsync(internalVariables.feURL))
            {
                if (reactDevServerProc == null || reactDevServerProc.HasExited)
                {
                    reactDevServerProc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "start",
                        WorkingDirectory = paths.feReactProjectPath,
                        UseShellExecute = true, // false
                        CreateNoWindow = true
                    });
                }

                // wait until server run
                await WaitUntilAliveAsync(internalVariables.feURL, timeoutMs: 60000);

                internalVariables.reactServerStarted = true;
            }
        }
    }
}
