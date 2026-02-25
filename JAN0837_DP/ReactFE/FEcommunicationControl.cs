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
                        CrossroadData.btnStart = value; 
                        break;
                    case "btnCrossroadPause": 
                        CrossroadData.btnPause = value; 
                        break;
                    case "btnCrossroadStop": 
                        CrossroadData.btnStop = value; 
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

        private static void ApplyCrosswalkUpdate(string key, string value)
        {
            CrosswalkData.Update(() =>
            {
                switch (key)
                {
                    case "crosswalkType": 
                        CrosswalkData.crosswalkType = value; 
                        break;
                    case "btnCrosswalkStart": 
                        CrosswalkData.btnCrosswalkStart = value; 
                        break;
                    case "btnCrosswalkPause": 
                        CrosswalkData.btnCrosswalkPause = value; 
                        break;
                    case "btnCrosswalkStop": 
                        CrosswalkData.btnCrosswalkStop = value; 
                        break;
                    case "btnCrosswalk1_crosswalk": 
                        CrosswalkData.btnCrosswalk1 = value; 
                        break;
                    case "btnCrosswalk2_crosswalk": 
                        CrosswalkData.btnCrosswalk2 = value; 
                        break;
                    case "trafficLight1_green_crosswalk": 
                        CrosswalkData.trafficLight1_green = value; 
                        break;
                    case "trafficLight1_yellow_crosswalk": 
                        CrosswalkData.trafficLight1_yellow = value; 
                        break;
                    case "trafficLight1_red_crosswalk": 
                        CrosswalkData.trafficLight1_red = value; 
                        break;
                    case "trafficLight2_green_crosswalk": 
                        CrosswalkData.trafficLight2_green = value; 
                        break;
                    case "trafficLight2_yellow_crosswalk": 
                        CrosswalkData.trafficLight2_yellow = value; 
                        break;
                    case "trafficLight2_red_crosswalk": 
                        CrosswalkData.trafficLight2_red = value; 
                        break;
                    case "pedestrian1_green_crosswalk": 
                        CrosswalkData.pedestrian1_green = value; 
                        break;
                    case "pedestrian1_red_crosswalk": 
                        CrosswalkData.pedestrian1_red = value; 
                        break;
                    case "pedestrian2_green_crosswalk": 
                        CrosswalkData.pedestrian2_green = value; 
                        break;
                    case "pedestrian2_red_crosswalk": 
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
                    case "switchstate_regulator": 
                        RegulatorData.switchstate = value; 
                        break;
                    case "R": 
                        RegulatorData.R = value; 
                        break;
                    case "C": 
                        RegulatorData.C = value; 
                        break;
                    case "U": 
                        RegulatorData.U = value; 
                        break;
                    case "Td": 
                        RegulatorData.Td = value; 
                        break;
                    case "Uc":
                        RegulatorData.Uc = value;
                        break;
                }
            });
        }

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

        private static void ApplyWashingMachineUpdate(string key, string value)
        {
            WashingMachineData.Update(() =>
            {
                switch (key)
                {
                    case "btnWashingMachineEmergencyStop": 
                        WashingMachineData.btnWashingMachineEmergencyStop = value; 
                        break;
                    case "btnStartWashingMachine": 
                        WashingMachineData.btnStartWashingMachine = value; 
                        break;
                    case "btnStopWashingMachine": 
                        WashingMachineData.btnStopWashingMachine = value; 
                        break;
                    case "WashingMachineErrorSystem": 
                        WashingMachineData.WashingMachineErrorSystem = value; 
                        break;
                    case "WashingMachineMode": 
                        WashingMachineData.WashingMachineMode = value; 
                        break;
                    case "WashingMachineLight_green": 
                        WashingMachineData.WashingMachineLight_green = value; 
                        break;
                    case "WashingMachineLight_yellow": 
                        WashingMachineData.WashingMachineLight_yellow = value; 
                        break;
                    case "WashingMachineLight_red": 
                        WashingMachineData.WashingMachineLight_red = value; 
                        break;
                    case "WashingMachineDoorClosed": 
                        WashingMachineData.WashingMachineDoorClosed = value; 
                        break;
                    case "WashingMachineChemicals": 
                        WashingMachineData.WashingMachineChemicals = value; 
                        break;
                    case "WashingMachinePrewash": 
                        WashingMachineData.WashingMachinePrewash = value; 
                        break;
                    case "WashingMachineWater": 
                        WashingMachineData.WashingMachineWater = value;
                        break;
                    case "WashingMachineDry": 
                        WashingMachineData.WashingMachineDry = value; 
                        break;
                    case "WashingMachineBrushes": 
                        WashingMachineData.WashingMachineBrushes = value; 
                        break;
                    case "WashingMachineSoap": 
                        WashingMachineData.WashingMachineSoap = value; 
                        break;
                    case "WashingMachineActiveFoam": 
                        WashingMachineData.WashingMachineActiveFoam = value; 
                        break;
                }
            });
        }

        private static void ApplyCarLightUpdate(string key, string value)
        {
            CarLightData.Update(() =>
            {
                switch (key)
                {
                    case "btnStart_carlight":
                        CarLightData.btnStart = value;
                        break;
                    case "btnReset_carlight":
                        CarLightData.btnReset = value;
                        break;
                    case "markerLight":
                        CarLightData.markerLight = value;
                        break;
                    case "brakeLight":
                        CarLightData.brakeLight = value;
                        break;
                    case "turnLight":
                        CarLightData.turnLight = value;
                        break;
                    case "sensorPosition":
                        CarLightData.sensorPosition = value;
                        break;
                    case "sensorConnectorConnected":
                        CarLightData.sensorConnectorConnected = value;
                        break;
                    case "done_carlight":
                        CarLightData.done = value;
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
                    var crosswalkdata = CrosswalkData.Get();
                    var regulatordata = RegulatorData.Get();
                    var carwashdata = CarWashData.Get();
                    var washingmachinedata = WashingMachineData.Get();
                    var carlightdata = CarLightData.Get();

                    WriteJSON(resp, new
                    {
                        TestData = new
                        {
                            number = testdata.number,
                            text = testdata.text,
                            toggle = testdata.toggle
                        },
                        CrossroadData = new
                        {
                            crossroadType = crossroaddata.crossroadType,
                            btnCrossroadStart = crossroaddata.btnStart,
                            btnCrossroadPause = crossroaddata.btnPause,
                            btnCrossroadStop = crossroaddata.btnStop,
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
                        },
                        CrosswalkData = new
                        {
                            crosswalkType = crosswalkdata.crosswalkType,
                            btnCrosswalkStart = crosswalkdata.btnCrosswalkStart,
                            btnCrosswalkPause = crosswalkdata.btnCrosswalkPause,
                            btnCrosswalkStop = crosswalkdata.btnCrosswalkStop,
                            btnCrosswalk1_crosswalk = crosswalkdata.btnCrosswalk1,
                            btnCrosswalk2_crosswalk = crosswalkdata.btnCrosswalk2,
                            trafficLight1_green_crosswalk = crosswalkdata.trafficLight1_green,
                            trafficLight1_yellow_crosswalk = crosswalkdata.trafficLight1_yellow,
                            trafficLight1_red_crosswalk = crosswalkdata.trafficLight1_red,
                            trafficLight2_green_crosswalk = crosswalkdata.trafficLight2_green,
                            trafficLight2_yellow_crosswalk = crosswalkdata.trafficLight2_yellow,
                            trafficLight2_red_crosswalk = crosswalkdata.trafficLight2_red,
                            pedestrian1_green_crosswalk = crosswalkdata.pedestrian1_green,
                            pedestrian1_red_crosswalk = crosswalkdata.pedestrian1_red,
                            pedestrian2_green_crosswalk = crosswalkdata.pedestrian2_green,
                            pedestrian2_red_crosswalk = crosswalkdata.pedestrian2_red
                        },
                        RegulatorData = new
                        {
                            switchstate_regulator = regulatordata.switchstate,
                            R = regulatordata.R,
                            C = regulatordata.C,
                            U = regulatordata.U,
                            Td = regulatordata.Td
                        },
                        CarWash = new
                        {
                            btnCarWashEmergencyStop = carwashdata.btnEmergencyStop,
                            btnStartCarWash = carwashdata.btnStart,
                            btnStopCarWash = carwashdata.btnStop,
                            CarWashErrorSystem = carwashdata.ErrorSystem,
                            CarWashCarPosition = carwashdata.CarPosition,
                            CarWashShowerPosition = carwashdata.ShowerPosition,
                            CarWashMode = carwashdata.Mode,
                            CarWashLight_green = carwashdata.Light_green,
                            CarWashLight_yellow = carwashdata.Light_yellow,
                            CarWashLight_red = carwashdata.Light_red,
                            CarWashDoor1_Up = carwashdata.Door1_Up,
                            CarWashDoor1_Down = carwashdata.Door1_Down,
                            CarWashDoor2_Up = carwashdata.Door2_Up,
                            CarWashDoor2_Down = carwashdata.Door2_Down,
                            CarWashChemicalsFront = carwashdata.ChemicalsFront,
                            CarWashChemicalsSides = carwashdata.ChemicalsSides,
                            CarWashChemicalsBack = carwashdata.ChemicalsBack,
                            CarWashPrewash = carwashdata.Prewash,
                            CarWashWater = carwashdata.Water,
                            CarWashWax = carwashdata.Wax,
                            CarWashDry = carwashdata.Dry,
                            CarWashBrushes = carwashdata.Brushes,
                            CarWashSoap = carwashdata.Soap,
                            CarWashActiveFoam = carwashdata.ActiveFoam,
                            CarWashTimeDoorMovement = carwashdata.TimeDoorMovement,
                            CarWashMEMDoor = carwashdata.MEMDoor,
                            CarWashMEMDoorTrig = carwashdata.MEMDoorTrig,
                            CarWashMEMDoorClosingtrig = carwashdata.MEMDoorClosingtrig
                        },
                        WashingMachine = new
                        {
                            btnWashingMachineEmergencyStop = washingmachinedata.btnWashingMachineEmergencyStop,
                            btnStartWashingMachine = washingmachinedata.btnStartWashingMachine,
                            btnStopWashingMachine = washingmachinedata.btnStopWashingMachine,
                            WashingMachineErrorSystem = washingmachinedata.WashingMachineErrorSystem,
                            WashingMachineMode = washingmachinedata.WashingMachineMode,
                            WashingMachineLight_green = washingmachinedata.WashingMachineLight_green,
                            WashingMachineLight_yellow = washingmachinedata.WashingMachineLight_yellow,
                            WashingMachineLight_red = washingmachinedata.WashingMachineLight_red,
                            WashingMachineDoorClosed = washingmachinedata.WashingMachineDoorClosed,
                            WashingMachineChemicals = washingmachinedata.WashingMachineChemicals,
                            WashingMachinePrewash = washingmachinedata.WashingMachinePrewash,
                            WashingMachineWater = washingmachinedata.WashingMachineWater,
                            WashingMachineDry = washingmachinedata.WashingMachineDry,
                            WashingMachineBrushes = washingmachinedata.WashingMachineBrushes,
                            WashingMachineSoap = washingmachinedata.WashingMachineSoap,
                            WashingMachineActiveFoam = washingmachinedata.WashingMachineActiveFoam
                        },
                        CarLight = new
                        {
                            btnStart_carlight = carlightdata.btnStart,
                            btnReset_carlight = carlightdata.btnReset,
                            markerLight = carlightdata.markerLight,
                            brakeLight = carlightdata.brakeLight,
                            turnLight = carlightdata.turnLight
                        }
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

                    // Apply updates to all data classes -> this will need huge upgrade -> for now it is totally wrong 
                    foreach (var kv in updates)
                    {
                        var key = kv.Key;
                        var value = kv.Value ?? "";

                        // CrossroadData
                        if (key == "crossroadType" || key.StartsWith("btnCrossroad") || key.StartsWith("btnCrosswalk") ||
                            key.StartsWith("trafficLight") && !key.Contains("crosswalk") || 
                            key.StartsWith("pedestrian") && !key.Contains("crosswalk"))
                        {
                            ApplyCrossroadUpdate(key, value);
                        }
                        // CrosswalkData (has suffix to avoid collision with CrossroadData)
                        else if (key == "crosswalkType" || key.StartsWith("btnCrosswalk") && key.Contains("crosswalk") ||
                                 key.Contains("_crosswalk"))
                        {
                            ApplyCrosswalkUpdate(key, value);
                        }
                        // RegulatorData
                        else if (key == "switchstate_regulator" || key == "R" || key == "C" || key == "U" || key == "Td" || key == "Uc")
                        {
                            ApplyRegulatorUpdate(key, value);
                        }
                        // CarWashData
                        else if (key.StartsWith("CarWash") || key.StartsWith("btnCarWash") || 
                                 key.StartsWith("btnStart") && key.Contains("CarWash") ||
                                 key.StartsWith("btnStop") && key.Contains("CarWash"))
                        {
                            ApplyCarWashUpdate(key, value);
                        }
                        // WashingMachineData
                        else if (key.StartsWith("WashingMachine") || key.StartsWith("btnWashingMachine") ||
                                 key.StartsWith("btnStart") && key.Contains("WashingMachine") ||
                                 key.StartsWith("btnStop") && key.Contains("WashingMachine"))
                        {
                            ApplyWashingMachineUpdate(key, value);
                        }
                        // CarLightData
                        else if (key.Contains("_carlight") || key == "markerLight" || key == "brakeLight" || 
                                 key == "turnLight" || key == "sensorPosition" || key == "sensorConnectorConnected")
                        {
                            ApplyCarLightUpdate(key, value);
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
            var testdata = TestData.AppState.Get();
            var crossroaddata = CrossroadData.Get();
            var crosswalkdata = CrosswalkData.Get();
            var regulatordata = RegulatorData.Get();
            var carwashdata = CarWashData.Get();
            var washingmachinedata = WashingMachineData.Get();
            var carlightdata = CarLightData.Get();

            return new
            {
                TestData = new
                {
                    text = testdata.text,
                    number = testdata.number,
                    toggle = testdata.toggle
                },
                CrossroadData = new
                {
                    crossroadType = crossroaddata.crossroadType,
                    btnCrossroadStart = crossroaddata.btnStart,
                    btnCrossroadPause = crossroaddata.btnPause,
                    btnCrossroadStop = crossroaddata.btnStop,
                    btnCrosswalk1 = crossroaddata.btnCrosswalk1,
                    btnCrosswalk2 = crossroaddata.btnCrosswalk2
                },
                CrosswalkData = new
                {
                    crosswalkType = crosswalkdata.crosswalkType,
                    btnCrosswalkStart = crosswalkdata.btnCrosswalkStart,
                    btnCrosswalkPause = crosswalkdata.btnCrosswalkPause,
                    btnCrosswalkStop = crosswalkdata.btnCrosswalkStop,
                    btnCrosswalk1_crosswalk = crosswalkdata.btnCrosswalk1,
                    btnCrosswalk2_crosswalk = crosswalkdata.btnCrosswalk2
                },
                RegulatorData = new
                {
                    switchstate_regulator = regulatordata.switchstate,
                    R = regulatordata.R,
                    C = regulatordata.C,
                    U = regulatordata.U,
                    Td = regulatordata.Td
                },
                CarWash = new
                {
                    btnCarWashEmergencyStop = carwashdata.btnEmergencyStop,
                    btnStartCarWash = carwashdata.btnStart,
                    btnStopCarWash = carwashdata.btnStop,
                    CarWashErrorSystem = carwashdata.ErrorSystem,
                    CarWashCarPosition = carwashdata.CarPosition,
                    CarWashShowerPosition = carwashdata.ShowerPosition,
                    CarWashMode = carwashdata.Mode
                },
                WashingMachine = new
                {
                    btnWashingMachineEmergencyStop = washingmachinedata.btnWashingMachineEmergencyStop,
                    btnStartWashingMachine = washingmachinedata.btnStartWashingMachine,
                    btnStopWashingMachine = washingmachinedata.btnStopWashingMachine,
                    WashingMachineErrorSystem = washingmachinedata.WashingMachineErrorSystem,
                    WashingMachineMode = washingmachinedata.WashingMachineMode
                },
                CarLight = new
                {
                    btnStart_carlight = carlightdata.btnStart,
                    btnReset_carlight = carlightdata.btnReset,
                    markerLight = carlightdata.markerLight,
                    brakeLight = carlightdata.brakeLight,
                    turnLight = carlightdata.turnLight
                }
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
            return GetDataAsync<TestData>(internalVariables.internalApiDataURL);
        }

        public Task<CrossroadData.State> GetCrossroadDataAsync()
        {
            return GetDataAsync<CrossroadData.State>(internalVariables.internalApiDataURL);
        }

        public Task<CrosswalkData.State> GetCrosswalkDataAsync()
        {
            return GetDataAsync<CrosswalkData.State>(internalVariables.internalApiDataURL);
        }

        public Task<RegulatorData.State> GetRegulatorDataAsync()
        {
            return GetDataAsync<RegulatorData.State>(internalVariables.internalApiDataURL);
        }

        public Task<CarWashData.State> GetCarWashDataAsync()
        {
            return GetDataAsync<CarWashData.State>(internalVariables.internalApiDataURL);
        }

        public Task<WashingMachineData.State> GetWashingMachineDataAsync()
        {
            return GetDataAsync<WashingMachineData.State>(internalVariables.internalApiDataURL);
        }

        public Task<CarLightData.State> GetCarLightDataAsync()
        {
            return GetDataAsync<CarLightData.State>(internalVariables.internalApiDataURL);
        }

        public void ApplySnapshot(TestData snap)
        {
            TestData.AppState.Set(snap);
        }

        public void ApplySnapshot(CrossroadData.State snap)
        {
            CrossroadData.Set(snap);
        }

        public void ApplySnapshot(CrosswalkData.State snap)
        {
            CrosswalkData.Set(snap);
        }

        public void ApplySnapshot(RegulatorData.State snap)
        {
            RegulatorData.Set(snap);
        }

        public void ApplySnapshot(CarWashData.State snap)
        {
            CarWashData.Set(snap);
        }

        public void ApplySnapshot(WashingMachineData.State snap)
        {
            WashingMachineData.Set(snap);
        }

        public void ApplySnapshot(CarLightData.State snap)
        {
            CarLightData.Set(snap);
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
