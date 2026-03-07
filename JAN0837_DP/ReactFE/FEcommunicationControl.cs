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
                $"http://+:{internalVariables.apiPort}/api/",                   
                $"http://*:{internalVariables.apiPort}/api/",                    
                $"http://{internalVariables.LocalIP}:{internalVariables.apiPort}/api/", 
                $"http://localhost:{internalVariables.apiPort}/api/"            
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

        public void Update(string category, string key, string value)
        {
            switch (category)
            {
                /*
                case "TestData":
                    var testdata = TestData.AppState.Get();
                    switch (key)
                    {
                        case "text": testdata.text = value; break;
                        case "number":
                            if (int.TryParse(value, out var n)) testdata.number = n;
                            break;
                        case "toggle": testdata.toggle = value; break;
                    }
                    TestData.AppState.Set(testdata);
                    break;
                */
                case "CrossroadData":
                    ApplyCrossroadUpdate(key, value);
                    break;
                case "CrosswalkData":
                    ApplyCrosswalkUpdate(key, value);
                    break;
                case "RegulatorData":
                    ApplyRegulatorUpdate(key, value);
                    break;
                /*
                case "CarWash":
                    ApplyCarWashUpdate(key, value);
                    break;
                */
                /*
                case "WashingMachine":
                    ApplyWashingMachineUpdate(key, value);
                    break;
                */
                case "CarLight":
                    ApplyCarLightUpdate(key, value);
                    break;
            }
        }

        private static void ApplyCrossroadUpdate(string key, string value)
        {
            CrossroadData.Update(() =>
            {
                switch (key)
                {
                    case "btnStart": 
                        CrossroadData.btnStart = value; 
                        break;
                    case "btnPause": 
                        CrossroadData.btnPause = value; 
                        break;
                    case "btnStop": 
                        CrossroadData.btnStop = value; 
                        break;
                    case "btnWestCrosswalk1": 
                        CrossroadData.btnWestCrosswalk1 = value; 
                        break;
                    case "btnWestCrosswalk2": 
                        CrossroadData.btnWestCrosswalk2 = value; 
                        break;
                    case "btnSouthCrosswalk1": 
                        CrossroadData.btnSouthCrosswalk1 = value; 
                        break;
                    case "btnSouthCrosswalk2": 
                        CrossroadData.btnSouthCrosswalk2 = value; 
                        break;
                    case "crossroadType": 
                        CrossroadData.crossroadType = value; 
                        break;
                    case "trafficLightNorth_green": 
                        CrossroadData.trafficLightNorth_green = value; 
                        break;
                    case "trafficLightNorth_yellow": 
                        CrossroadData.trafficLightNorth_yellow = value; 
                        break;
                    case "trafficLightNorth_red": 
                        CrossroadData.trafficLightNorth_red = value; 
                        break;
                    case "trafficLightSouth_green": 
                        CrossroadData.trafficLightSouth_green = value; 
                        break;
                    case "trafficLightSouth_yellow": 
                        CrossroadData.trafficLightSouth_yellow = value; 
                        break;
                    case "trafficLightSouth_red": 
                        CrossroadData.trafficLightSouth_red = value; 
                        break;
                    case "trafficLightWest_green": 
                        CrossroadData.trafficLightWest_green = value; 
                        break;
                    case "trafficLightWest_yellow": 
                        CrossroadData.trafficLightWest_yellow = value;
                        break;
                    case "trafficLightWest_red": 
                        CrossroadData.trafficLightWest_red = value; 
                        break;
                    case "trafficLightEast_green": 
                        CrossroadData.trafficLightEast_green = value;
                        break;
                    case "trafficLightEast_yellow": 
                        CrossroadData.trafficLightEast_yellow = value; 
                        break;
                    case "trafficLightEast_red": 
                        CrossroadData.trafficLightEast_red = value; 
                        break;
                    case "pedestrianSouth1_green": 
                        CrossroadData.pedestrianSouth1_green = value; 
                        break;
                    case "pedestrianSouth1_red": 
                        CrossroadData.pedestrianSouth1_red = value;
                        break;
                    case "pedestrianSouth2_green":
                        CrossroadData.pedestrianSouth2_green = value;
                        break;
                    case "pedestrianSouth2_red":
                        CrossroadData.pedestrianSouth2_red = value;
                        break;
                    case "pedestrianWest1_green": 
                        CrossroadData.pedestrianWest1_green = value; 
                        break;
                    case "pedestrianWest1_red": 
                        CrossroadData.pedestrianWest1_red = value; 
                        break;
                    case "pedestrianWest2_green":
                        CrossroadData.pedestrianWest2_green = value;
                        break;
                    case "pedestrianWest2_red":
                        CrossroadData.pedestrianWest2_red = value;
                        break;
                }
            });
        }

        private static void ApplyCrosswalkUpdate(string key, string value)
        {
            CrosswalkData.Update(() =>
            {
                switch (key)
                {
                    case "btnStart": 
                        CrosswalkData.btnStart = value; 
                        break;
                    case "btnPause": 
                        CrosswalkData.btnPause = value; 
                        break;
                    case "btnStop": 
                        CrosswalkData.btnStop = value; 
                        break;
                    case "btnCrosswalk1": 
                        CrosswalkData.btnCrosswalk1 = value; 
                        break;
                    case "btnCrosswalk2": 
                        CrosswalkData.btnCrosswalk2 = value; 
                        break;
                    case "crosswalkType":
                        CrosswalkData.crosswalkType = value;
                        break;
                    case "trafficLight1_green": 
                        CrosswalkData.trafficLight1_green = value; 
                        break;
                    case "trafficLight1_yellow": 
                        CrosswalkData.trafficLight1_yellow = value; 
                        break;
                    case "trafficLight1_red": 
                        CrosswalkData.trafficLight1_red = value; 
                        break;
                    case "trafficLight2_green": 
                        CrosswalkData.trafficLight2_green = value; 
                        break;
                    case "trafficLight2_yellow": 
                        CrosswalkData.trafficLight2_yellow = value; 
                        break;
                    case "trafficLight2_red": 
                        CrosswalkData.trafficLight2_red = value; 
                        break;
                    case "pedestrian1_green": 
                        CrosswalkData.pedestrian1_green = value; 
                        break;
                    case "pedestrian1_red": 
                        CrosswalkData.pedestrian1_red = value; 
                        break;
                    case "pedestrian2_green": 
                        CrosswalkData.pedestrian2_green = value; 
                        break;
                    case "pedestrian2_red": 
                        CrosswalkData.pedestrian2_red = value; 
                        break;
                }
            });
        }

        private static void ApplyRegulatorUpdate(string key, string value)
        {
            RegulatorData.Update(() =>
            {
                switch (key)
                {
                    case "btnReset":
                        RegulatorData.btnReset = value;
                        break;
                    case "switchstate": 
                        RegulatorData.switchstate = value; 
                        break;
                    case "order": 
                        RegulatorData.order = value; 
                        break;
                    case "R1": 
                        RegulatorData.R1 = value; 
                        break;
                    case "R2":
                        RegulatorData.R2 = value;
                        break;
                    case "C1": 
                        RegulatorData.C1 = value; 
                        break;
                    case "C2":
                        RegulatorData.C2 = value;
                        break;
                    case "Td": 
                        RegulatorData.Td = value; 
                        break;
                    case "Ts": 
                        RegulatorData.Ts = value; 
                        break;
                    case "Uc1":
                        RegulatorData.Uc1 = value;
                        break;
                    case "Uc2":
                        RegulatorData.Uc2 = value;
                        break;
                    case "Uin":
                        RegulatorData.Uin = value;
                        break;
                }
            });
        }

        #region CarWash & WashingMachine 
        /*
        private static void ApplyCarWashUpdate(string key, string value)
        {
            CarWashData.Update(() =>
            {
                switch (key)
                {
                    case "btnEmergencyStop": 
                        CarWashData.btnEmergencyStop = value; 
                        break;
                    case "btnStart": 
                        CarWashData.btnStart = value; 
                        break;
                    case "btnStop": 
                        CarWashData.btnStop = value; 
                        break;
                    case "ErrorSystem": 
                        CarWashData.ErrorSystem = value; 
                        break;
                    case "CarPosition": 
                        CarWashData.CarPosition = value; 
                        break;
                    case "ShowerPosition": 
                        CarWashData.ShowerPosition = value; 
                        break;
                    case "Mode": 
                        CarWashData.Mode = value; 
                        break;
                    case "Light_green": 
                        CarWashData.Light_green = value; 
                        break;
                    case "Light_yellow": 
                        CarWashData.Light_yellow = value; 
                        break;
                    case "Light_red": 
                        CarWashData.Light_red = value; 
                        break;
                    case "Door1_Up": 
                        CarWashData.Door1_Up = value; 
                        break;
                    case "Door1_Down": 
                        CarWashData.Door1_Down = value; 
                        break;
                    case "Door2_Up": 
                        CarWashData.Door2_Up = value; 
                        break;
                    case "Door2_Down": 
                        CarWashData.Door2_Down = value; 
                        break;
                    case "ChemicalsFront": 
                        CarWashData.ChemicalsFront = value; 
                        break;
                    case "ChemicalsSides": 
                        CarWashData.ChemicalsSides = value; 
                        break;
                    case "ChemicalsBack": 
                        CarWashData.ChemicalsBack = value; 
                        break;
                    case "Prewash": 
                        CarWashData.Prewash = value; 
                        break;
                    case "Water": 
                        CarWashData.Water = value; 
                        break;
                    case "Wax": 
                        CarWashData.Wax = value; 
                        break;
                    case "Dry": 
                        CarWashData.Dry = value; 
                        break;
                    case "Brushes": 
                        CarWashData.Brushes = value; 
                        break;
                    case "Soap": 
                        CarWashData.Soap = value; 
                        break;
                    case "ActiveFoam": 
                        CarWashData.ActiveFoam = value; 
                        break;
                    case "TimeDoorMovement": 
                        CarWashData.TimeDoorMovement = value; 
                        break;
                    case "MEMDoor": 
                        CarWashData.MEMDoor = value; 
                        break;
                    case "MEMDoorTrig": 
                        CarWashData.MEMDoorTrig = value; 
                        break;
                    case "MEMDoorClosingtrig": 
                        CarWashData.MEMDoorClosingtrig = value; 
                        break;
                }
            });
        }
        */
        /*
        private static void ApplyWashingMachineUpdate(string key, string value)
        {
            WashingMachineData.Update(() =>
            {
                switch (key)
                {
                    case "btnEmergencyStop": 
                        WashingMachineData.btnEmergencyStop = value; 
                        break;
                    case "btnStart": 
                        WashingMachineData.btnStart = value; 
                        break;
                    case "btnStop": 
                        WashingMachineData.btnStop = value; 
                        break;
                    case "ErrorSystem": 
                        WashingMachineData.ErrorSystem = value; 
                        break;
                    case "Mode": 
                        WashingMachineData.Mode = value; 
                        break;
                    case "Light_green": 
                        WashingMachineData.Light_green = value; 
                        break;
                    case "Light_yellow": 
                        WashingMachineData.Light_yellow = value; 
                        break;
                    case "Light_red": 
                        WashingMachineData.Light_red = value; 
                        break;
                    case "DoorClosed": 
                        WashingMachineData.DoorClosed = value; 
                        break;
                    case "Chemicals": 
                        WashingMachineData.Chemicals = value; 
                        break;
                    case "Prewash": 
                        WashingMachineData.Prewash = value; 
                        break;
                    case "Water": 
                        WashingMachineData.Water = value;
                        break;
                    case "Dry": 
                        WashingMachineData.Dry = value; 
                        break;
                    case "Brushes": 
                        WashingMachineData.Brushes = value; 
                        break;
                    case "Soap": 
                        WashingMachineData.Soap = value; 
                        break;
                    case "ActiveFoam": 
                        WashingMachineData.ActiveFoam = value; 
                        break;
                }
            });
        }
        */
        #endregion

        private static void ApplyCarLightUpdate(string key, string value)
        {
            CarLightData.Update(() =>
            {
                switch (key)
                {
                    case "btnReset":
                        CarLightData.btnReset = value;
                        break;
                    case "error":
                        CarLightData.error = value;
                        break;
                    case "sensorLight":
                        CarLightData.sensorLight = value;
                        break;
                    case "sensorConnectorConnected":
                        CarLightData.sensorConnectorConnected = value;
                        break;
                    case "lowBeamLight":
                        CarLightData.lowBeamLight = value;
                        break;
                    case "highBeamLight":
                        CarLightData.highBeamLight = value;
                        break;
                    case "turnLight":
                        CarLightData.turnLight = value;
                        break;
                    case "result":
                        CarLightData.result = value;
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
                // ── Swagger UI ──
                if (req.HttpMethod == "GET" && (path == "/swagger" || path == "/swagger/index.html"))
                {
                    var host = req.Url.Authority;
                    var specUrl = $"http://{host}/api/swagger/openapi.json";
                    var html = SwaggerContent.GetSwaggerHtml(specUrl);
                    var buf = Encoding.UTF8.GetBytes(html);
                    resp.ContentType = "text/html; charset=utf-8";
                    resp.ContentLength64 = buf.Length;
                    resp.OutputStream.Write(buf, 0, buf.Length);
                    resp.Close();
                    return;
                }

                if (req.HttpMethod == "GET" && path == "/swagger/openapi.json")
                {
                    var host = req.Url.Authority;
                    var json = SwaggerContent.GetOpenApiSpec(host);
                    var buf = Encoding.UTF8.GetBytes(json);
                    resp.ContentType = "application/json; charset=utf-8";
                    resp.ContentLength64 = buf.Length;
                    resp.OutputStream.Write(buf, 0, buf.Length);
                    resp.Close();
                    return;
                }

                // ── Data endpoints --  
                if (req.HttpMethod == "GET" && (path == "/data" || path == "/"))
                {
                    //var testdata = TestData.AppState.Get();
                    var crossroaddata = CrossroadData.Get();
                    var crosswalkdata = CrosswalkData.Get();
                    var regulatordata = RegulatorData.Get();
                    //var carwashdata = CarWashData.Get();
                    //var washingmachinedata = WashingMachineData.Get();
                    var carlightdata = CarLightData.Get();
                    
                    // all data (inputs & outputs)
                    WriteJSON(resp, new
                    {
                        /*
                        TestData = new
                        {
                            number = testdata.number,
                            text = testdata.text,
                            toggle = testdata.toggle
                        },
                        */
                        CrossroadData = new
                        {
                            btnStart = crossroaddata.btnStart,
                            btnPause = crossroaddata.btnPause,
                            btnStop = crossroaddata.btnStop,
                            crossroadType = crossroaddata.crossroadType,
                            btnWestCrosswalk1 = crossroaddata.btnWestCrosswalk1,
                            btnWestCrosswalk2 = crossroaddata.btnWestCrosswalk2,
                            btnSouthCrosswalk1 = crossroaddata.btnSouthCrosswalk1,
                            btnSouthCrosswalk2 = crossroaddata.btnSouthCrosswalk2,
                            trafficLightNorth_green = crossroaddata.trafficLightNorth_green,
                            trafficLightNorth_yellow = crossroaddata.trafficLightNorth_yellow,
                            trafficLightNorth_red = crossroaddata.trafficLightNorth_red,
                            trafficLightSouth_green = crossroaddata.trafficLightSouth_green,
                            trafficLightSouth_yellow = crossroaddata.trafficLightSouth_yellow,
                            trafficLightSouth_red = crossroaddata.trafficLightSouth_red,
                            trafficLightWest_green = crossroaddata.trafficLightWest_green,
                            trafficLightWest_yellow = crossroaddata.trafficLightWest_yellow,
                            trafficLightWest_red = crossroaddata.trafficLightWest_red,
                            trafficLightEast_green = crossroaddata.trafficLightEast_green,
                            trafficLightEast_yellow = crossroaddata.trafficLightEast_yellow,
                            trafficLightEast_red = crossroaddata.trafficLightEast_red,
                            pedestrianSouth1_green = crossroaddata.pedestrianSouth1_green,
                            pedestrianSouth1_red = crossroaddata.pedestrianSouth1_red,
                            pedestrianSouth2_green = crossroaddata.pedestrianSouth2_green,
                            pedestrianSouth2_red = crossroaddata.pedestrianSouth2_red,
                            pedestrianWest1_green = crossroaddata.pedestrianWest1_green,
                            pedestrianWest1_red = crossroaddata.pedestrianWest1_red,
                            pedestrianWest2_green = crossroaddata.pedestrianWest2_green,
                            pedestrianWest2_red = crossroaddata.pedestrianWest2_red,
                        },
                        CrosswalkData = new
                        {
                            btnStart = crosswalkdata.btnStart,
                            btnPause = crosswalkdata.btnPause,
                            btnStop = crosswalkdata.btnStop,
                            crosswalkType = crosswalkdata.crosswalkType,
                            btnCrosswalk1 = crosswalkdata.btnCrosswalk1,
                            btnCrosswalk2 = crosswalkdata.btnCrosswalk2,
                            trafficLight1_green = crosswalkdata.trafficLight1_green,
                            trafficLight1_yellow = crosswalkdata.trafficLight1_yellow,
                            trafficLight1_red = crosswalkdata.trafficLight1_red,
                            trafficLight2_green = crosswalkdata.trafficLight2_green,
                            trafficLight2_yellow = crosswalkdata.trafficLight2_yellow,
                            trafficLight2_red = crosswalkdata.trafficLight2_red,
                            pedestrian1_green = crosswalkdata.pedestrian1_green,
                            pedestrian1_red = crosswalkdata.pedestrian1_red,
                            pedestrian2_green = crosswalkdata.pedestrian2_green,
                            pedestrian2_red = crosswalkdata.pedestrian2_red
                        },
                        RegulatorData = new
                        {
                            btnReset = regulatordata.btnReset,
                            switchstate = regulatordata.switchstate,
                            order = regulatordata.order,
                            R1 = regulatordata.R1,
                            R2 = regulatordata.R2,
                            C1 = regulatordata.C1,
                            C2 = regulatordata.C2,
                            Uc1 = regulatordata.Uc1,
                            Uc2 = regulatordata.Uc2,
                            Td = regulatordata.Td,
                            Ts = regulatordata.Ts,
                            Uin = regulatordata.Uin
                        },
                        /*
                        CarWash = new
                        {
                            btnEmergencyStop = carwashdata.btnEmergencyStop,
                            btnStart = carwashdata.btnStart,
                            btnStop = carwashdata.btnStop,
                            ErrorSystem = carwashdata.ErrorSystem,
                            CarPosition = carwashdata.CarPosition,
                            ShowerPosition = carwashdata.ShowerPosition,
                            Mode = carwashdata.Mode,
                            Light_green = carwashdata.Light_green,
                            Light_yellow = carwashdata.Light_yellow,
                            Light_red = carwashdata.Light_red,
                            Door1_Up = carwashdata.Door1_Up,
                            Door1_Down = carwashdata.Door1_Down,
                            Door2_Up = carwashdata.Door2_Up,
                            Door2_Down = carwashdata.Door2_Down,
                            ChemicalsFront = carwashdata.ChemicalsFront,
                            ChemicalsSides = carwashdata.ChemicalsSides,
                            ChemicalsBack = carwashdata.ChemicalsBack,
                            Prewash = carwashdata.Prewash,
                            Water = carwashdata.Water,
                            Wax = carwashdata.Wax,
                            Dry = carwashdata.Dry,
                            Brushes = carwashdata.Brushes,
                            Soap = carwashdata.Soap,
                            ActiveFoam = carwashdata.ActiveFoam,
                            TimeDoorMovement = carwashdata.TimeDoorMovement,
                            MEMDoor = carwashdata.MEMDoor,
                            MEMDoorTrig = carwashdata.MEMDoorTrig,
                            MEMDoorClosingtrig = carwashdata.MEMDoorClosingtrig
                        },
                        */
                        /*
                        WashingMachine = new
                        {
                            btnEmergencyStop = washingmachinedata.btnEmergencyStop,
                            btnStart = washingmachinedata.btnStart,
                            btnStop = washingmachinedata.btnStop,
                            ErrorSystem = washingmachinedata.ErrorSystem,
                            Mode = washingmachinedata.Mode,
                            Light_green = washingmachinedata.Light_green,
                            Light_yellow = washingmachinedata.Light_yellow,
                            Light_red = washingmachinedata.Light_red,
                            DoorClosed = washingmachinedata.DoorClosed,
                            Chemicals = washingmachinedata.Chemicals,
                            Prewash = washingmachinedata.Prewash,
                            Water = washingmachinedata.Water,
                            Dry = washingmachinedata.Dry,
                            Brushes = washingmachinedata.Brushes,
                            Soap = washingmachinedata.Soap,
                            ActiveFoam = washingmachinedata.ActiveFoam
                        },
                        */
                        CarLight = new
                        {
                            btnReset = carlightdata.btnReset,
                            error = carlightdata.error,
                            sensorLight = carlightdata.sensorLight,
                            sensorConnectorConnected = carlightdata.sensorConnectorConnected,
                            lowBeamLight = carlightdata.lowBeamLight,
                            highBeamLight = carlightdata.highBeamLight,
                            turnLight = carlightdata.turnLight,
                            result = carlightdata.result
                        }
                    });
                    return;
                }
                else if (req.HttpMethod == "POST" && (path == "/data" || path == "/"))
                {
                    using var sr = new StreamReader(req.InputStream);
                    var body = sr.ReadToEnd();

                    // Nested JSON: { "CrossroadData": { "btnStart": "true" }, "CarWash": { "Mode": "1" }, ... }
                    var categories = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(body)
                        ?? new Dictionary<string, Dictionary<string, string>>();

                    foreach (var category in categories)
                    {
                        foreach (var kv in category.Value)
                        {
                            Update(category.Key, kv.Key, kv.Value ?? "");
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
                    Logger.LogError("FE HandleRequest - Method Not Allowed: " + req.HttpMethod + " " + path);
                    return;
                }
            }
            catch (Exception ex)
            {
                // jednoduchý JSON error (ať to líp debuguješ v Network panelu)
                Logger.LogException(ex, "FECommuncationControl HandleRequest");
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
            //var testdata = TestData.AppState.Get();
            var crossroaddata = CrossroadData.Get();
            var crosswalkdata = CrosswalkData.Get();
            var regulatordata = RegulatorData.Get();
            //var carwashdata = CarWashData.Get();
            //var washingmachinedata = WashingMachineData.Get();
            var carlightdata = CarLightData.Get();

            // Only Inputs 
            return new
            {
                /*
                TestData = new
                {
                    text = testdata.text,
                    number = testdata.number,
                    toggle = testdata.toggle
                },
                */
                CrossroadData = new
                {
                    //crossroadType = crossroaddata.crossroadType,
                    btnStart = crossroaddata.btnStart,
                    btnPause = crossroaddata.btnPause,
                    btnStop = crossroaddata.btnStop,
                    btnWestCrosswalk1 = crossroaddata.btnWestCrosswalk1,
                    btnWestCrosswalk2 = crossroaddata.btnWestCrosswalk2,
                    btnSouthCrosswalk1 = crossroaddata.btnSouthCrosswalk1,
                    btnSouthCrosswalk2 = crossroaddata.btnSouthCrosswalk2
                },
                CrosswalkData = new
                {
                    //crosswalkType = crosswalkdata.crosswalkType,
                    btnStart = crosswalkdata.btnStart,
                    btnPause = crosswalkdata.btnPause,
                    btnStop = crosswalkdata.btnStop,
                    btnCrosswalk1_crosswalk = crosswalkdata.btnCrosswalk1,
                    btnCrosswalk2_crosswalk = crosswalkdata.btnCrosswalk2
                },
                RegulatorData = new
                {
                    btnReset = regulatordata.btnReset,
                    switchstate = regulatordata.switchstate,
                    order = regulatordata.order,
                    R1 = regulatordata.R1,
                    R2 = regulatordata.R2,
                    C1 = regulatordata.C1,
                    C2 = regulatordata.C2,
                    Uc1 = regulatordata.Uc1,
                    Uc2 = regulatordata.Uc2,
                    Td = regulatordata.Td,
                    Ts = regulatordata.Ts
                },
                /*
                CarWash = new
                {
                    btnEmergencyStop = carwashdata.btnEmergencyStop,
                    btnStart = carwashdata.btnStart,
                    btnStop = carwashdata.btnStop,
                    ErrorSystem = carwashdata.ErrorSystem,
                    CarPosition = carwashdata.CarPosition,
                    ShowerPosition = carwashdata.ShowerPosition,
                    Mode = carwashdata.Mode
                },
                */
                /*
                WashingMachine = new
                {
                    btnEmergencyStop = washingmachinedata.btnEmergencyStop,
                    btnStart = washingmachinedata.btnStart,
                    btnStop = washingmachinedata.btnStop,
                    ErrorSystem = washingmachinedata.ErrorSystem,
                    Mode = washingmachinedata.Mode
                },
                */
                CarLight = new
                {
                    btnReset = carlightdata.btnReset,
                    error = carlightdata.error,
                    sensorLight = carlightdata.sensorLight,
                    sensorConnectorConnected = carlightdata.sensorConnectorConnected,
                    //lowBeamLight = carlightdata.lowBeamLight,
                    //highBeamLight = carlightdata.highBeamLight,
                    //turnLight = carlightdata.turnLight
                }
            };
        }

        public void HandleUpdate(string category, Dictionary<string, string> updates)
        {
            foreach (var kv in updates)
                Update(category, kv.Key, kv.Value ?? "");
        }

        public async Task<T> GetDataAsync<T>(string url)
        {
            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json) ?? Activator.CreateInstance<T>();
        }

        public async Task<bool> IsAliveAsync(string url)
        {
            try
            {
                using var resp = await healthCheckClient.GetAsync(url);
                return resp.IsSuccessStatusCode;
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogException(ex, $"Health check timeout for {url}");
                return false;
            }
            catch (HttpRequestException httpex)
            {
                Logger.LogException(httpex, $"Health check error for {url}");
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
                if (await IsAliveAsync(url))
                {
                    return;
                }
                await Task.Delay(pollMs);
            }
            
            var errorMsg = $"Service not reachable after {timeoutMs}ms: {url}";
            Logger.LogError(errorMsg);
            throw new TimeoutException(errorMsg);
        }

        public async Task EnsureCommunicationServiceAsync()
        {
            var apiHealth = internalVariables.internalApiDataURL;

            if (!await IsAliveAsync(apiHealth))
            {
                await WaitUntilAliveAsync(apiHealth, timeoutMs: 5000);
            }
        }

        public async Task EnsureReactDevServerAsync()
        {
            var internalFeUrl = internalVariables.internalFeURL;
            
            if (await IsAliveAsync(internalFeUrl))
            {
                Console.WriteLine("React FE server is already running!");
                internalVariables.feServerStarted = true;
                return;
            }
            
            if (reactDevServerProc == null || reactDevServerProc.HasExited)
            {
                var reactPath = paths.feReactProjectPath;
                Console.WriteLine($"React project path: {reactPath}");
                
                if (!Directory.Exists(reactPath))
                {
                    var errorMsg = $"React project directory not found: {reactPath}";
                    Console.WriteLine($"ERROR: {errorMsg}");
                    Logger.LogError(errorMsg);
                    throw new DirectoryNotFoundException(errorMsg);
                }
                
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
                    Arguments = $"/k set \"HOST=0.0.0.0\" && set \"PORT={internalVariables.fePort}\" && npm start",
                    WorkingDirectory = reactPath,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                          
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

            var startTime = Environment.TickCount;
            var timeoutMs = 120000;
            var pollMs = 2000;
            
            while (Environment.TickCount - startTime < timeoutMs)
            {
                if (await IsAliveAsync(internalFeUrl))
                {
                    internalVariables.feServerStarted = true;
                    return;
                }
                
                var elapsed = (Environment.TickCount - startTime) / 1000;
                Console.WriteLine($"  Waiting... ({elapsed}s)");
                await Task.Delay(pollMs);
            }
            
            var errorMessage = $"React dev server did not start within {timeoutMs / 1000} seconds. Check the console window for npm errors.";
            var err = new TimeoutException(errorMessage);
            Logger.LogException(err, "EnsureReactDevServerAsync");
            throw err;
        }
    }
}
