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
        private string _prefix;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public Process reactDevServerProc;

        public static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(1500) };
        
        // Separate client for health checks with longer timeout
        private static readonly HttpClient healthCheckClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public FEcommunicationControl(string prefix)
        {
            // Will be set in communicationStart based on what works
            _prefix = $"http://localhost:{internalVariables.apiPort}/api/";
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

            _listener = new HttpListener();
            
            // Try different prefixes in order of preference
            string[] prefixesToTry = new[]
            {
                $"http://+:{internalVariables.apiPort}/api/",                    // All interfaces (requires admin/urlacl)
                $"http://*:{internalVariables.apiPort}/api/",                    // Alternative all interfaces
                $"http://{internalVariables.LocalIP}:{internalVariables.apiPort}/api/",  // Specific IP
                $"http://localhost:{internalVariables.apiPort}/api/"             // Localhost only (always works)
            };

            bool started = false;
            foreach (var prefix in prefixesToTry)
            {
                try
                {
                    _listener.Prefixes.Clear();
                    _listener.Prefixes.Add(prefix);
                    _listener.Start();
                    _prefix = prefix;
                    started = true;
                    
                    // Extract the host from the prefix for internal health checks
                    if (prefix.Contains("localhost"))
                    {
                        internalVariables.actualApiHost = "localhost";
                        Console.WriteLine("WARNING: Server only accessible from this machine!");
                        Console.WriteLine("Run as Admin or use: netsh http add urlacl url=http://+:5000/ user=Everyone");
                        Logger.LogWarning("API Server bound to localhost only - not accessible from network");
                    }
                    else if (prefix.Contains("+") || prefix.Contains("*"))
                    {
                        // Bound to all interfaces - use localhost for internal checks
                        internalVariables.actualApiHost = "localhost";
                        Console.WriteLine($"Accessible at: {internalVariables.communicationBaseURL}");
                    }
                    else
                    {
                        // Bound to specific IP
                        internalVariables.actualApiHost = internalVariables.LocalIP;
                        Console.WriteLine($"Accessible at: {internalVariables.communicationBaseURL}");
                    }
                    
                    Console.WriteLine($"API Server started on {prefix}");
                    //Logger.LogInfo($"API Server started on {prefix}");
                    break;
                }
                catch (HttpListenerException ex)
                {
                    Console.WriteLine($"Failed to bind to {prefix}: {ex.Message}");
                    Logger.LogWarning($"Failed to bind to {prefix}: {ex.Message}");
                    
                    // Close and recreate listener for next attempt
                    try { _listener.Close(); } catch { }
                    _listener = new HttpListener();
                }
            }

            if (!started)
            {
                throw new Exception("Could not start HTTP listener on any prefix. Check firewall and permissions.");
            }

            _cts = new CancellationTokenSource();
            Task.Run(() => HandleAsync(_cts.Token));

            internalVariables.communicationServerStarted = true;
        }

        public void communicationStop()
        {
            if (internalVariables.communicationServerStarted == true)
            {
                try
                {
                    // Cancel the token first to signal the handler to stop
                    _cts?.Cancel();
                    
                    // Small delay to let the handler exit gracefully
                    Thread.Sleep(100);
                    
                    // Now stop and close the listener
                    if (_listener != null)
                    {
                        try
                        {
                            if (_listener.IsListening)
                            {
                                _listener.Stop();
                            }
                            _listener.Close();
                        }
                        catch (ObjectDisposedException) { /* Already disposed */ }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "FE communicationStop");
                }
                finally
                {
                    _listener = null;
                    _cts?.Dispose();
                    _cts = null;
                    internalVariables.communicationServerStarted = false;
                }
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
                try
                {
                    // Check if listener is still valid
                    if (_listener == null || !_listener.IsListening)
                    {
                        break;
                    }
                    
                    var ctx = await _listener.GetContextAsync();
                    HandleRequest(ctx);
                }
                catch (ObjectDisposedException)
                {
                    // Listener was disposed - exit gracefully
                    break;
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995) // ERROR_OPERATION_ABORTED
                {
                    // Listener was stopped - exit gracefully
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "FE HandleAsync");
                    // Continue listening for other requests
                }
            }
        }

        public void AddCors(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var origin = (req.Headers["Origin"] ?? "").TrimEnd('/');

            // Allow any origin for development - echo back the requesting origin
            if (!string.IsNullOrEmpty(origin))
            {
                resp.Headers["Access-Control-Allow-Origin"] = origin;
            }
            else
            {
                // Fallback to allow the configured FE URL
                resp.Headers["Access-Control-Allow-Origin"] = internalVariables.feURL;
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

            // Log incoming request for debugging
            var clientIP = req.RemoteEndPoint?.Address?.ToString() ?? "unknown";
            //Logger.LogInfo($"HTTP Request from {clientIP}: {req.HttpMethod} {req.Url?.AbsoluteUri}");
            Console.WriteLine($"[HTTP] {clientIP} -> {req.HttpMethod} {req.Url?.AbsolutePath}");

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
            // Use internal URL for same-machine requests
            return GetDataAsync<TestData>(internalVariables.internalApiDataURL);
        }

        public Task<CrossroadData.State> GetCrossroadDataAsync()
        {
            // Use internal URL for same-machine requests
            return GetDataAsync<CrossroadData.State>(internalVariables.internalApiDataURL);
        }


        public async Task<TestData> GetDataAsync()
        {
            // Use internal URL for same-machine requests
            var url = internalVariables.internalApiDataURL;
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
                using var resp = await healthCheckClient.GetAsync(url);
                return resp.IsSuccessStatusCode;
            }
            catch (TaskCanceledException)
            {
                // Timeout
                return false;
            }
            catch (HttpRequestException)
            {
                // Connection failed
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Health check error for {url}: {ex.Message}");
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
            
            // Log the timeout
            var errorMsg = $"Service not reachable after {timeoutMs}ms: {url}";
            Logger.LogError(errorMsg);
            throw new TimeoutException(errorMsg);
        }

        public async Task EnsureCommunicationServiceAsync()
        {
            // API health-check using internal URL (works even if bound to localhost)
            var apiHealth = internalVariables.internalApiDataURL;
            
            Console.WriteLine($"Checking API health at: {apiHealth}");
            //Logger.LogInfo($"Checking API health at: {apiHealth}");

            if (!await IsAliveAsync(apiHealth))
            {
                await WaitUntilAliveAsync(apiHealth, timeoutMs: 5000);
            }
            
            Console.WriteLine("API service is alive!");
            //Logger.LogInfo("API service is alive!");
        }

        public async Task EnsureReactDevServerAsync()
        {
            // Use internal URL for health check (localhost works even if server binds to 0.0.0.0)
            var internalFeUrl = internalVariables.internalFeURL;
            
            Console.WriteLine($"Checking React FE at: {internalFeUrl}");
            //Logger.LogInfo($"Checking React FE at: {internalFeUrl}");
            
            if (await IsAliveAsync(internalFeUrl))
            {
                Console.WriteLine("React FE server is already running!");
                //Logger.LogInfo("React FE server is already running!");
                internalVariables.feServerStarted = true;
                return;
            }
            
            // React not running - try to start it
            if (reactDevServerProc == null || reactDevServerProc.HasExited)
            {
                // Verify the React project path exists
                var reactPath = paths.feReactProjectPath;
                Console.WriteLine($"React project path: {reactPath}");
                //Logger.LogInfo($"React project path: {reactPath}");
                
                if (!Directory.Exists(reactPath))
                {
                    var errorMsg = $"React project directory not found: {reactPath}";
                    Console.WriteLine($"ERROR: {errorMsg}");
                    Logger.LogError(errorMsg);
                    throw new DirectoryNotFoundException(errorMsg);
                }
                
                // Check if package.json exists
                var packageJson = Path.Combine(reactPath, "package.json");
                if (!File.Exists(packageJson))
                {
                    var errorMsg = $"package.json not found at: {packageJson}";
                    Console.WriteLine($"ERROR: {errorMsg}");
                    Logger.LogError(errorMsg);
                    throw new FileNotFoundException(errorMsg);
                }
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    // Use quotes around SET values to avoid trailing spaces
                    Arguments = $"/k set \"HOST=0.0.0.0\" && set \"PORT={internalVariables.fePort}\" && npm start",
                    WorkingDirectory = reactPath,
                    UseShellExecute = true,
                    CreateNoWindow = false  // Show window so you can see React output
                };
                
                Console.WriteLine($"Starting React dev server...");
                Console.WriteLine($"  Command: {startInfo.Arguments}");
                Console.WriteLine($"  Working dir: {startInfo.WorkingDirectory}");
                Console.WriteLine($"  Port: {internalVariables.fePort}");
                Console.WriteLine("  NOTE: A command window will open. Wait for 'Compiled successfully!'");
                //Logger.LogInfo($"Starting React dev server on port {internalVariables.fePort}");
                
                try
                {
                    reactDevServerProc = Process.Start(startInfo);
                    Console.WriteLine($"React process started with PID: {reactDevServerProc?.Id}");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed to start React dev server");
                    throw;
                }
            }

            // Wait using internal URL (localhost) with progress logging
            Console.WriteLine($"Waiting for React server to be ready at {internalFeUrl}...");
            Console.WriteLine("(This can take 30-60 seconds on first run)");
            var startTime = Environment.TickCount;
            var timeoutMs = 120000;  // 2 minutes timeout for first compile
            var pollMs = 2000;
            
            while (Environment.TickCount - startTime < timeoutMs)
            {
                if (await IsAliveAsync(internalFeUrl))
                {
                    internalVariables.feServerStarted = true;
                    Console.WriteLine($"React FE server is running!");
                    Console.WriteLine($"External access: {internalVariables.feURL}");
                    //Logger.LogInfo($"React FE server started - External: {internalVariables.feURL}");
                    return;
                }
                
                var elapsed = (Environment.TickCount - startTime) / 1000;
                Console.WriteLine($"  Waiting... ({elapsed}s)");
                await Task.Delay(pollMs);
            }
            
            // Timeout - log detailed info
            var errorMessage = $"React server not responding after 120s at {internalFeUrl}";
            Console.WriteLine($"ERROR: {errorMessage}");
            Console.WriteLine($"Check the npm window for errors!");
            Console.WriteLine($"You can also start React manually:");
            Console.WriteLine($"  cd \"{paths.feReactProjectPath}\"");
            Console.WriteLine($"  npm start");
            
            // Log all details to log file
            Logger.LogError(errorMessage);
            Logger.LogError($"React project path: {paths.feReactProjectPath}");
            Logger.LogError($"Expected URL: {internalFeUrl}");
            Logger.LogError($"Process PID: {reactDevServerProc?.Id}");
            Logger.LogError($"Process HasExited: {reactDevServerProc?.HasExited}");
            //Logger.LogInfo("Hint: Start React manually with: npm start");
            
            var err = new TimeoutException(errorMessage);
            Logger.LogException(err, "EnsureReactDevServerAsync");
            throw err;
        }
    }
}
