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
using JAN0837_DP.Log;
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

            _prefix = (basePrefix ?? "http://192.168.1.250:5000/api/").TrimEnd('/') + "/";

            // var prefix = (internalVariables.communicationBaseURL ?? "http://localhost:5000/api/").TrimEnd('/') + "/";
        }

        public void communicationStart()
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

        public void communicationStop()
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
            var testdata = TestData.AppState.Get();

            string newValue = Convert.ToString(value) ?? "";

            switch (key)
            {
                case "status":
                    testdata.text = Convert.ToString(value) ?? "";
                    break;

                case "parameter1":
                    if (value is int i)
                    {
                        testdata.number = i;
                    }
                    else if (value is long l)
                    {
                        testdata.number = (int)l;
                    }
                    else if (int.TryParse(Convert.ToString(value), out var n))
                    {
                        testdata.number = n;
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

                case "toggle":
                    ApplyCrossroadUpdate(key, newValue);
                    break;

                case "crossroadType":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "btnCrossroadStart":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "btnCrossroadPause":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "btnCrossroadStop":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "btnCrosswalk1":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "btnCrosswalk2":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "trafficLight1_green":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "trafficLight1_yellow":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "trafficLight1_red":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "trafficLight2_green":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "trafficLight2_yellow":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "trafficLight2_red":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "pedestrian1_green":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "pedestrian1_red":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "pedestrian2_green":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
                case "pedestrian2_red":
                    ApplyCrossroadUpdate(key, newValue);
                    break;
            }

            TestData.AppState.Set(testdata);
        }

        private static void ApplyCrossroadUpdate(string key, string value)
        {
            CrossroadData.Update(() =>
            {
                switch (key)
                {
                    case "crossroadType": 
                        CrossroadData.crossroadType = value; 
                        break;

                    case "btnCrossroadStart": 
                        CrossroadData.btnCrossroadStart = value; 
                        break;
                    case "btnCrossroadPause": 
                        CrossroadData.btnCrossroadPause = value; 
                        break;
                    case "btnCrossroadStop": 
                        CrossroadData.btnCrossroadStop = value; 
                        break;

                    case "btnCrosswalk1": 
                        CrossroadData.btnCrosswalk1 = value; 
                        break;
                    case "btnCrosswalk2": 
                        CrossroadData.btnCrosswalk2 = value; 
                        break;

                    case "trafficLight1_green": 
                        CrossroadData.trafficLight1_green = value; 
                        break;
                    case "trafficLight1_yellow": 
                        CrossroadData.trafficLight1_yellow = value; 
                        break;
                    case "trafficLight1_red": 
                        CrossroadData.trafficLight1_red = value; 
                        break;

                    case "trafficLight2_green": 
                        CrossroadData.trafficLight2_green = value; 
                        break;
                    case "trafficLight2_yellow": 
                        CrossroadData.trafficLight2_yellow = value; 
                        break;
                    case "trafficLight2_red": CrossroadData.trafficLight2_red = value; break;

                    case "pedestrian1_green": 
                        CrossroadData.pedestrian1_green = value; 
                        break;
                    case "pedestrian1_red": 
                        CrossroadData.pedestrian1_red = value; 
                        break;
                    case "pedestrian2_green": 
                        CrossroadData.pedestrian2_green = value; 
                        break;
                    case "pedestrian2_red": 
                        CrossroadData.pedestrian2_red = value; 
                        break;
                }
            });
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
            var allowedOrigin = (internalVariables.feURL ?? "http://192.168.1.250:3000").TrimEnd('/');
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
                    var testdata = TestData.AppState.Get();
                    var crossroaddata = CrossroadData.Get();

                    WriteJSON(resp, new
                    {
                        // TestData
                        number = testdata.number,
                        text = testdata.text,
                        toggle = testdata.toggle,

                        // CrossroadData
                        crossroadType = crossroaddata.crossroadType,
                        btnCrossroadStart = crossroaddata.btnCrossroadStart,
                        btnCrossroadPause = crossroaddata.btnCrossroadPause,
                        btnCrossroadStop = crossroaddata.btnCrossroadStop,
                        btnCrosswalk1 = crossroaddata.btnCrosswalk1,
                        btnCrosswalk2 = crossroaddata.btnCrosswalk2,
                        trafficLight1_green = crossroaddata.trafficLight1_green,
                        trafficLight1_yellow = crossroaddata.trafficLight1_yellow,
                        trafficLight1_red = crossroaddata.trafficLight1_red,
                        trafficLight2_green = crossroaddata.trafficLight2_green,
                        trafficLight2_yellow = crossroaddata.trafficLight2_yellow,
                        trafficLight2_red = crossroaddata.trafficLight2_red,
                        pedestrian1_green = crossroaddata.pedestrian1_green,
                        pedestrian1_red = crossroaddata.pedestrian1_red,
                        pedestrian2_green = crossroaddata.pedestrian2_green,
                        pedestrian2_red = crossroaddata.pedestrian2_red
                    });
                    return;
                }
                else if (req.HttpMethod == "POST" && (path == "/data" || path == "/"))
                {
                    using var sr = new StreamReader(req.InputStream);
                    var body = sr.ReadToEnd();

                    var updates = JsonConvert.DeserializeObject<Dictionary<string, string>>(body) ?? new Dictionary<string, string>();

                    var testdata = TestData.AppState.Get();

                    // TestData
                    if (updates.TryGetValue("number", out var ns) && int.TryParse(ns, out var n))
                    {
                        testdata.number = Convert.ToInt32(n);
                    }

                    if (updates.TryGetValue("text", out var t))
                    {
                        testdata.text = Convert.ToString(t);
                    }

                    if (updates.TryGetValue("toggle", out var g))
                    {
                        testdata.toggle = Convert.ToString(g);
                    }

                    TestData.AppState.Set(testdata);

                    // CrossroadData
                    foreach (var kv in updates)
                    {
                        switch (kv.Key)
                        {
                            case "crossroadType":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "btnCrossroadStart":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "btnCrossroadPause":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "btnCrossroadStop":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "btnCrosswalk1":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "btnCrosswalk2":   
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "trafficLight1_green":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "trafficLight1_yellow":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "trafficLight1_red":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "trafficLight2_green":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "trafficLight2_yellow":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "trafficLight2_red":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "pedestrian1_green":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "pedestrian1_red":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "pedestrian2_green":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                            case "pedestrian2_red":
                                ApplyCrossroadUpdate(kv.Key, kv.Value ?? "");
                                break;
                        }
                    }

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
                Logger.LogException(ex, "FE HandleRequest");
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
            var testdata = TestData.AppState.Get();
            var crossroaddata = CrossroadData.Get();

            return new
            {
                // TestData
                text = testdata.text,
                number = testdata.number,
                toggle = testdata.toggle,

                // CrossroadData
                //crossroadType = crossroaddata.crossroadType,
                btnCrossroadStart = crossroaddata.btnCrossroadStart,
                btnCrossroadPause = crossroaddata.btnCrossroadPause,
                btnCrossroadStop = crossroaddata.btnCrossroadStop,
                btnCrosswalk1 = crossroaddata.btnCrosswalk1,
                btnCrosswalk2 = crossroaddata.btnCrosswalk2//,
                //trafficLight1_green = crossroaddata.trafficLight1_green,
                //trafficLight1_yellow = crossroaddata.trafficLight1_yellow,
                //trafficLight1_red = crossroaddata.trafficLight1_red,
                //trafficLight2_green = crossroaddata.trafficLight2_green,
                //trafficLight2_yellow = crossroaddata.trafficLight2_yellow,
                //trafficLight2_red = crossroaddata.trafficLight2_red,
                //pedestrian1_green = crossroaddata.pedestrian1_green,
                //pedestrian1_red = crossroaddata.pedestrian1_red,
                //pedestrian2_green = crossroaddata.pedestrian2_green,
                //pedestrian2_red = crossroaddata.pedestrian2_red
            };
        }

        public void HandleUpdate(Dictionary<string, object> updates)
        {
            foreach (var kv in updates)
                Update(kv.Key, kv.Value);
        }

        public async Task<T> GetDataAsync<T>(string url)
        {
            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json) ?? Activator.CreateInstance<T>();
        }

        public Task<TestData> GetTestDataAsync()
        {
            return GetDataAsync<TestData>(internalVariables.communicationDataURL);
        }

        public Task<CrossroadData.State> GetCrossroadDataAsync()
        {
            return GetDataAsync<CrossroadData.State>(internalVariables.communicationDataURL);
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

            CrossroadData.Set(snap);

            Update("crossroadType", snap.crossroadType);
            Update("btnCrossroadStart", snap.btnCrossroadStart);
            Update("btnCrossroadPause", snap.btnCrossroadPause);
            Update("btnCrossroadStop", snap.btnCrossroadStop);
            Update("btnCrosswalk1", snap.btnCrosswalk1);
            Update("btnCrosswalk2", snap.btnCrosswalk2);
            Update("trafficlight1_green", snap.trafficlight1_green);
            Update("trafficLight1_yellow", snap.trafficLight1_yellow);
            Update("trafficLight1_red", snap.trafficLight1_red);
            Update("trafficLight2_green", snap.trafficLight2_green);
            Update("trafficLight2_yellow", snap.trafficLight2_yellow);
            Update("trafficLight2_red", snap.trafficLight2_red);
            Update("pedestrian1_green", snap.pedestrian1_green);
            Update("pedestrian1_red", snap.pedestrian1_red);
            Update("pedestrian2_green", snap.pedestrian2_green);
            Update("pedestrian2_red", snap.pedestrian2_red);
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

                internalVariables.feServerStarted = true;
            }
        }
    }
}
