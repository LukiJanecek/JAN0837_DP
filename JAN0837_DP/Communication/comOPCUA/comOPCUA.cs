// OPCUA
using Opc;
using Opc.Ua;
using Opc.Ua.Buffers;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua.Export;
using Opc.Ua.Security;
using Opc.Ua.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using JAN0837_DP.Log;

namespace JAN0837_DP.Communication.comOPCUA
{
    public class opcuaServer
    {
        private ApplicationInstance _application;
        private CrossroadOpcUaServer _server;
        public bool running = false;
                
        public async Task<bool> startOPCUAserver(string ipAddress, int port)
        {
            if (running)
            {
                return true;
            }

            try
            {               
                string serverUrl = $"opc.tcp://{ipAddress}:{port}";

                // Create application configuration
                var config = new Opc.Ua.ApplicationConfiguration()
                {
                    ApplicationName = "JAN0837_DP_OPC_UA_Server",
                    ApplicationType = Opc.Ua.ApplicationType.Server,
                    ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:JAN0837_DP:OPCUAServer",
                    ProductUri = "http://jan0837/opcuaserver",
                    
                    SecurityConfiguration = new Opc.Ua.SecurityConfiguration
                    {
                        ApplicationCertificate = new Opc.Ua.CertificateIdentifier
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "own")
                        },
                        TrustedPeerCertificates = new Opc.Ua.CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "trusted")
                        },
                        TrustedIssuerCertificates = new Opc.Ua.CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "issuer")
                        },
                        RejectedCertificateStore = new Opc.Ua.CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "rejected")
                        },
                        AutoAcceptUntrustedCertificates = true,
                        RejectSHA1SignedCertificates = false,
                        MinimumCertificateKeySize = 1024,
                        AddAppCertToTrustedStore = true
                    },
                    
                    ServerConfiguration = new Opc.Ua.ServerConfiguration
                    {
                        BaseAddresses = new Opc.Ua.StringCollection { serverUrl },
                        MinRequestThreadCount = 5,
                        MaxRequestThreadCount = 100,
                        MaxQueuedRequestCount = 200,
                        
                        // Add alternative URLs for flexibility
                        AlternateBaseAddresses = new Opc.Ua.StringCollection()
                    },
                    
                    TransportQuotas = new Opc.Ua.TransportQuotas 
                    { 
                        OperationTimeout = 600000,
                        MaxStringLength = 1048576,
                        MaxByteStringLength = 1048576,
                        MaxArrayLength = 65535,
                        MaxMessageSize = 4194304,
                        MaxBufferSize = 65535,
                        ChannelLifetime = 300000,
                        SecurityTokenLifetime = 3600000
                    },
                    
                    TraceConfiguration = new Opc.Ua.TraceConfiguration
                    {
                        OutputFilePath = Path.Combine(Path.GetTempPath(), "JAN0837_Server.log"),
                        TraceMasks = 1
                    }
                };

                // Validate configuration
                await config.Validate(Opc.Ua.ApplicationType.Server);

                // Create application instance
                _application = new ApplicationInstance
                {
                    ApplicationConfiguration = config,
                    ApplicationType = Opc.Ua.ApplicationType.Server
                };

                // Ensure certificate directories exist
                var pkiRoot = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki");
                Directory.CreateDirectory(Path.Combine(pkiRoot, "own"));
                Directory.CreateDirectory(Path.Combine(pkiRoot, "trusted"));
                Directory.CreateDirectory(Path.Combine(pkiRoot, "issuer"));
                Directory.CreateDirectory(Path.Combine(pkiRoot, "rejected"));

                // Load or create certificate
                var certIdentifier = config.SecurityConfiguration.ApplicationCertificate;
                var certificate = await certIdentifier.Find(true);
                
                if (certificate == null)
                {
                    Console.WriteLine("Creating new self-signed application certificate...");
                    
                    // Create certificate
                    certificate = CertificateFactory.CreateCertificate(
                        config.ApplicationUri,
                        config.ApplicationName,
                        "CN=" + config.ApplicationName,
                        null
                    ).CreateForRSA();
                    
                    // Set it in the configuration
                    config.SecurityConfiguration.ApplicationCertificate.Certificate = certificate;
                    
                    Console.WriteLine($"Certificate created with thumbprint: {certificate.Thumbprint}");
                }
                else
                {
                    Console.WriteLine($"Using existing certificate with thumbprint: {certificate.Thumbprint}");
                }

                // Create and start server
                _server = new CrossroadOpcUaServer();
                await _application.Start(_server);

                running = true;
                Console.WriteLine($"OPC UA Server started successfully on {serverUrl}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OPC UA Server Start");
                running = false;
                return false;
            }
        }

        public async Task<bool> stopOPCUAserver()
        {
            if (!running)
            {
                return true;
            }

            try
            {
                if (_server != null)
                {
                    _server.Stop();
                }

                running = false;
                Console.WriteLine("OPC UA Server stopped successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OPC UA Server Stop");
                return false;
            }
        }

        // Update variable value in the server
        public void UpdateVariable(string variableName, bool value)
        {
            if (_server != null && running)
            {
                _server.UpdateVariable(variableName, value);
            }
        }

        // Read variable value from the server (for inputs from clients)
        public bool ReadVariable(string variableName)
        {
            if (_server != null && running)
            {
                return _server.ReadVariable(variableName);
            }
            return false;
        }
    }

    public class opcuaKlient
    {
        public Opc.Ua.Client.Session clientSession;
        public Opc.Ua.Client.SessionReconnectHandler reconnectHandler;
        public bool connected = false;
        public bool running = false;    
        public CancellationTokenSource _cts;
        public ConcurrentQueue<(string nodeId, object value)> _writeQueue = new();
        public Opc.Ua.Client.Subscription subscription;
        public event KeepAliveEventHandler KeepAlive;

        // Store connection parameters for reconnection
        private string _lastServerUrl = string.Empty;
        private string _lastUser = string.Empty;
        private string _lastPass = string.Empty;
        private readonly SemaphoreSlim _reconnectLock = new(1, 1);
        private bool _isReconnecting = false;

        public async Task<bool> connectToOPCUAserver(string serverURL, string user, string pass)
        {
            // Store for potential reconnection
            _lastServerUrl = serverURL;
            _lastUser = user;
            _lastPass = pass;

            if (clientSession?.Connected == true)
            {
                return true;
            }

            try
            {
                var config = new Opc.Ua.ApplicationConfiguration()
                {
                    ApplicationName = "JAN0837_DP_OPC_UA_Client",
                    ApplicationType = Opc.Ua.ApplicationType.Client,
                    ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:JAN0837_DP:OPCUAClient",
                    ProductUri = "http://jan0837/opcuaclient",

                    SecurityConfiguration = new Opc.Ua.SecurityConfiguration
                    {
                        ApplicationCertificate = new Opc.Ua.CertificateIdentifier
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "own")
                        },
                        TrustedPeerCertificates = new Opc.Ua.CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "trusted")
                        },
                        TrustedIssuerCertificates = new Opc.Ua.CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "issuer")
                        },
                        RejectedCertificateStore = new Opc.Ua.CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki", "rejected")
                        },
                        AutoAcceptUntrustedCertificates = true,
                        RejectSHA1SignedCertificates = false,
                        MinimumCertificateKeySize = 1024,
                        AddAppCertToTrustedStore = true
                    },

                    TransportQuotas = new Opc.Ua.TransportQuotas
                    {
                        OperationTimeout = 600000,
                        MaxStringLength = 1048576,
                        MaxByteStringLength = 1048576,
                        MaxArrayLength = 65535,
                        MaxMessageSize = 4194304,
                        MaxBufferSize = 65535,
                        ChannelLifetime = 300000,
                        SecurityTokenLifetime = 3600000
                    },

                    ClientConfiguration = new Opc.Ua.ClientConfiguration
                    {
                        DefaultSessionTimeout = 60000,
                        MinSubscriptionLifetime = 10000
                    },

                    TraceConfiguration = new Opc.Ua.TraceConfiguration
                    {
                        OutputFilePath = Path.Combine(Path.GetTempPath(), "JAN0837_Client.log"),
                        TraceMasks = 515 // More detailed tracing
                    }
                };

                // 1. Ensure certificate directories exist FIRST
                var pkiRoot = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki");
                Directory.CreateDirectory(Path.Combine(pkiRoot, "own"));
                Directory.CreateDirectory(Path.Combine(pkiRoot, "trusted"));
                Directory.CreateDirectory(Path.Combine(pkiRoot, "issuer"));
                Directory.CreateDirectory(Path.Combine(pkiRoot, "rejected"));

                // 2. Validate configuration BEFORE anything else
                await config.Validate(Opc.Ua.ApplicationType.Client);

                // 3. Accept all certificates
                config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = true; };

                // 4. Check/create application certificate
                var hasAppCertificate = await config.SecurityConfiguration.ApplicationCertificate.Find(true);
                if (hasAppCertificate == null)
                {
                    Console.WriteLine("Creating new self-signed client certificate...");
                    var certificate = Opc.Ua.CertificateFactory.CreateCertificate(
                        config.ApplicationUri,
                        config.ApplicationName,
                        "CN=" + config.ApplicationName,
                        null
                    ).CreateForRSA();
                    config.SecurityConfiguration.ApplicationCertificate.Certificate = certificate;
                    Console.WriteLine($"Certificate created: {certificate.Thumbprint}");
                }

                // 5. Select endpoint based on authentication type
                Console.WriteLine($"Discovering endpoints at: {serverURL}");
                
                bool useCredentials = !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass);
                EndpointDescription endpointDescription;

                if (useCredentials)
                {
                    // When using username/password, we need a secure endpoint
                    // First, discover all endpoints and find one that supports UserName token
                    Console.WriteLine("Credentials provided - searching for secure endpoint with UserName support...");
                    
                    using var discoveryClient = DiscoveryClient.Create(new Uri(serverURL));
                    var endpoints = await discoveryClient.GetEndpointsAsync(null, CancellationToken.None);
                    
                    // Find endpoint with security that supports UserName authentication
                    endpointDescription = endpoints
                        .Where(e => 
                            e.SecurityMode != MessageSecurityMode.None &&
                            e.UserIdentityTokens.Any(t => t.TokenType == UserTokenType.UserName))
                        .OrderByDescending(e => e.SecurityLevel)
                        .FirstOrDefault();
                    
                    if (endpointDescription == null)
                    {
                        // Fallback: try any endpoint that supports UserName (even without encryption)
                        Console.WriteLine("No secure endpoint found, trying any endpoint with UserName support...");
                        endpointDescription = endpoints
                            .Where(e => e.UserIdentityTokens.Any(t => t.TokenType == UserTokenType.UserName))
                            .OrderByDescending(e => e.SecurityLevel)
                            .FirstOrDefault();
                    }
                    
                    if (endpointDescription == null)
                    {
                        throw new Exception("No endpoint found that supports UserName authentication. Check server configuration.");
                    }
                }
                else
                {
                    // Anonymous - use simple endpoint selection (no security required)
                    endpointDescription = await Opc.Ua.Client.CoreClientUtils.SelectEndpointAsync(
                        config,
                        serverURL,
                        false,
                        15000
                    );
                }

                Console.WriteLine($"Selected endpoint: {endpointDescription.EndpointUrl}");
                Console.WriteLine($"Security Mode: {endpointDescription.SecurityMode}");
                Console.WriteLine($"Security Policy: {endpointDescription.SecurityPolicyUri}");
                Console.WriteLine($"Supported tokens: {string.Join(", ", endpointDescription.UserIdentityTokens.Select(t => t.TokenType))}");

                var endpointConfiguration = Opc.Ua.EndpointConfiguration.Create(config);
                var endpoint = new Opc.Ua.ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                // 6. Create user identity
                Opc.Ua.IUserIdentity userIdentity;
                if (useCredentials)
                {
                    userIdentity = new Opc.Ua.UserIdentity(new Opc.Ua.UserNameIdentityToken
                    {
                        UserName = user,
                        Password = System.Text.Encoding.UTF8.GetBytes(pass)
                    });
                    Console.WriteLine($"Using username: {user}");
                }
                else
                {
                    userIdentity = new Opc.Ua.UserIdentity(new Opc.Ua.AnonymousIdentityToken());
                    Console.WriteLine("Using anonymous authentication");
                }

                // 7. Create session
                Console.WriteLine("Creating session...");
                clientSession = await Opc.Ua.Client.Session.Create(
                    config,
                    endpoint,
                    false,
                    "OPCUA Client Session",
                    60000,
                    userIdentity,
                    null
                );

                connected = clientSession.Connected;
                //clientSession.KeepAliveInterval = 5000;
                clientSession.KeepAlive += ClientSession_KeepAlive;

                Console.WriteLine($"SUCCESS! Connected to: {serverURL}");
                Console.WriteLine($"Session ID: {clientSession.SessionId}");
                return true;
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                Logger.LogError($"OPC UA Error: 0x{ex.StatusCode:X8} - {ex.Message}");
                connected = false;
                clientSession = null;
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OPC UA Connection");
                connected = false;
                clientSession = null;
                return false;
            }
        }

        public async Task<bool> disconnectFromOPCUAserver()
        {
            if (!connected)
            {
                return true;
            }

            try
            {
                if (reconnectHandler != null)
                {
                    reconnectHandler.Dispose();
                    reconnectHandler = null;
                }

                if (clientSession != null)
                {
                    clientSession.KeepAlive -= ClientSession_KeepAlive;

                    if (clientSession.Connected)
                        await clientSession.CloseAsync();

                    clientSession.Dispose();
                    clientSession = null;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void ClientSession_KeepAlive(Opc.Ua.Client.ISession session, KeepAliveEventArgs e)
        {
            // session hlásí problém
            if (ServiceResult.IsBad(e.Status))
            {
                // reconnect už běží
                if (reconnectHandler != null)
                {
                    return;
                }

                reconnectHandler = new SessionReconnectHandler();
                reconnectHandler.BeginReconnect((Opc.Ua.Client.Session)session, 5000, ClientSession_ReconnectComplete);
            }
        }

        private void ClientSession_ReconnectComplete(object? sender, EventArgs e)
        {
            /*
            if (reconnectHandler == null) return;

            // převezmi novou session
            clientSession = (Opc.Ua.Client.Session)reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;

            connected = clientSession?.Connected == true;
            */
            if (reconnectHandler?.Session is Opc.Ua.Client.Session newSession)
            {
                clientSession = newSession;
                connected = clientSession.Connected;
            }

            reconnectHandler?.Dispose();
            reconnectHandler = null;
        }

        private static void CertificateValidator_CertificateValidation(
        CertificateValidator sender,
        CertificateValidationEventArgs e)
        {
            e.Accept = true; // DEV ONLY
        }

        public async Task<bool> TryReconnectAsync()
        {
            if (string.IsNullOrEmpty(_lastServerUrl))
            {
                Console.WriteLine("Cannot reconnect: no previous connection parameters stored.");
                return false;
            }

            // Prevent concurrent reconnection attempts
            if (!await _reconnectLock.WaitAsync(0))
            {
                Console.WriteLine("Reconnection already in progress...");
                return false;
            }

            try
            {
                _isReconnecting = true;
                Console.WriteLine($"Attempting to reconnect to {_lastServerUrl}...");

                // Clean up old session
                if (clientSession != null)
                {
                    try
                    {
                        clientSession.KeepAlive -= ClientSession_KeepAlive;
                        if (clientSession.Connected)
                            await clientSession.CloseAsync();
                        clientSession.Dispose();
                    }
                    catch { /* Ignore cleanup errors */ }
                    clientSession = null;
                }

                connected = false;

                // Attempt reconnection
                bool success = await connectToOPCUAserver(_lastServerUrl, _lastUser, _lastPass);
                
                if (success)
                {
                    Console.WriteLine("Reconnection successful!");
                }
                else
                {
                    Console.WriteLine("Reconnection failed.");
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OPC UA Reconnection");
                return false;
            }
            finally
            {
                _isReconnecting = false;
                _reconnectLock.Release();
            }
        }

        public void WriteOPCUAValue(opcuaKlient client, string nodeId, object value)
        {
            try
            {
                // Validate session before writing
                if (!ValidateSession())
                {
                    Console.WriteLine($"Session invalid, cannot write to {nodeId}");
                    return;
                }

                var nodeIdParsed = NodeId.Parse(nodeId);

                // Optional: Pre-read to check node status (useful for debugging)
                client.clientSession.Read(
                    null,
                    0,
                    TimestampsToReturn.Neither,
                    new ReadValueIdCollection {
                        new ReadValueId { 
                            NodeId = nodeIdParsed, 
                            AttributeId = Attributes.Value 
                        }
                    },
                    out DataValueCollection readValues,
                    out _);

                if (StatusCode.IsBad(readValues[0].StatusCode))
                {
                    Console.WriteLine($"Warning: Node {nodeId} read status before write: {readValues[0].StatusCode}");
                }

                var wv = new WriteValue
                {
                    NodeId = nodeIdParsed,
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value))
                };

                var valuesToWrite = new WriteValueCollection { wv };
                client.clientSession.Write(null, valuesToWrite, out StatusCodeCollection results, out _);

                if (results.Count != 1 || StatusCode.IsBad(results[0]))
                {
                    Console.WriteLine($"Write failed for {nodeId}: 0x{results[0].Code:X8}");
                    throw new Exception($"Write failed for {nodeId}: {results.FirstOrDefault()}");
                }
            }
            catch (ServiceResultException ex)
            {
                // Handle session-related errors with reconnection
                if (ex.StatusCode == StatusCodes.BadSessionIdInvalid || 
                    ex.StatusCode == StatusCodes.BadSessionClosed ||
                    ex.StatusCode == StatusCodes.BadSessionNotActivated ||
                    ex.StatusCode == StatusCodes.BadSecureChannelClosed ||
                    ex.StatusCode == StatusCodes.BadConnectionClosed)
                {
                    Logger.LogError($"Session error writing to {nodeId}: [0x{ex.StatusCode:X8}] {ex.Message}");
                    connected = false;

                    // Trigger async reconnection (fire-and-forget, loop will retry)
                    if (!_isReconnecting)
                    {
                        _ = TryReconnectAsync();
                    }
                }
                else
                {
                    Logger.LogError($"OPC UA error writing to {nodeId}: [0x{ex.StatusCode:X8}] {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"OPC UA WriteValue {nodeId}");
            }
        }

        public bool ReadOPCUABool(opcuaKlient client, string nodeId)
        {
            try
            {
                // Validate session before reading
                if (!ValidateSession())
                {
                    Console.WriteLine($"Session invalid, cannot read from {nodeId}");
                    return false;
                }

                var id = NodeId.Parse(nodeId);

                client.clientSession.Read(
                    null,
                    0,
                    TimestampsToReturn.Neither,
                    new ReadValueIdCollection {
                        new ReadValueId { 
                            NodeId = id, 
                            AttributeId = Attributes.Value 
                        }
                    },
                    out DataValueCollection values,
                    out DiagnosticInfoCollection diag);

                Console.WriteLine($"Read status: {values[0].StatusCode}");

                var nodeIdParsed = NodeId.Parse(nodeId);

                DataValue value = client.clientSession.ReadValue(nodeIdParsed);

                if (value != null && value.Value != null)
                {
                    return Convert.ToBoolean(value.Value);
                }

                return false;
            }
            catch (ServiceResultException ex)
            {
                // Handle session-related errors with reconnection
                if (ex.StatusCode == StatusCodes.BadSessionIdInvalid || 
                    ex.StatusCode == StatusCodes.BadSessionClosed ||
                    ex.StatusCode == StatusCodes.BadSessionNotActivated ||
                    ex.StatusCode == StatusCodes.BadSecureChannelClosed ||
                    ex.StatusCode == StatusCodes.BadConnectionClosed)
                {
                    Logger.LogError($"Session error reading from {nodeId}: [0x{ex.StatusCode:X8}] {ex.Message}");
                    connected = false;

                    // Trigger async reconnection
                    if (!_isReconnecting)
                    {
                        _ = TryReconnectAsync();
                    }
                }
                else
                {
                    Logger.LogError($"OPC UA error reading from {nodeId}: [0x{ex.StatusCode:X8}] {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"OPC UA ReadBool {nodeId}");
                return false;
            }
        }

        // Validate and restore session if needed
        private bool ValidateSession()
        {
            if (clientSession == null)
            {
                return false;
            }

            if (!clientSession.Connected)
            {
                connected = false;
                return false;
            }

            // Check if KeepAlive is stopped and restart it
            if (clientSession.KeepAliveStopped)
            {
                Console.WriteLine("KeepAlive stopped, restarting...");
                try
                {
                    // Restart keep-alive by setting the interval again
                    clientSession.KeepAlive += ClientSession_KeepAlive;
                    clientSession.KeepAliveInterval = 5000;
                    
                    // The session will automatically start sending keep-alives
                    Console.WriteLine("KeepAlive restarted successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restart KeepAlive: {ex.Message}");
                    connected = false;
                    return false;
                }
            }

            return true;
        }

        ///
        public async Task<bool> connectToOPCUAserver_v2(string serverURL, string user, string pass)
        {
            if (clientSession?.Connected == true)
            {
                connected = true;
                return true;
            }

            try
            {
                var application = new ApplicationInstance
                {
                    ApplicationType = Opc.Ua.ApplicationType.Client,
                    ConfigSectionName = "Client"
                };

                await application.LoadApplicationConfiguration(false);

                var config = application.ApplicationConfiguration;
                await OpcUaConfigHelpers.EnsureApplicationCertificateAsync(config);
                config.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;

                // === ENDPOINT SELECTION ===
                EndpointDescription selectedEndpoint;

                // connectToOPCUAserver_v2: replace endpoint discovery + session create
                using var discoveryClient = DiscoveryClient.Create(new Uri(serverURL));
                var endpoints = await discoveryClient.GetEndpointsAsync(null, CancellationToken.None);

                selectedEndpoint = endpoints
                    .Where(e =>
                        e.SecurityMode != MessageSecurityMode.None &&
                        e.UserIdentityTokens.Any(t => t.TokenType == UserTokenType.UserName))
                    .OrderByDescending(e => e.SecurityLevel)
                    .FirstOrDefault()
                    ?? throw new Exception("No secure endpoint with UserName token found.");

                var endpoint = new ConfiguredEndpoint(
                    null,
                    selectedEndpoint,
                    EndpointConfiguration.Create(config)
                );

                IUserIdentity identity =
                    string.IsNullOrWhiteSpace(user)
                        ? new UserIdentity(new AnonymousIdentityToken())
                        : new UserIdentity(new UserNameIdentityToken
                        {
                            UserName = user,
                            Password = System.Text.Encoding.UTF8.GetBytes(pass ?? string.Empty)
                        });

                clientSession = await Opc.Ua.Client.Session.Create(
                    config,
                    endpoint,
                    false,
                    "OPCUA Client",
                    60000,
                    identity,
                    null);

                clientSession.KeepAlive += ClientSession_KeepAlive;

                connected = clientSession.Connected;
                return connected;
            }
            catch (Exception ex)
            {
                connected = false;
                clientSession = null;
                throw new Exception("OPCUA connect failed", ex);
            }
        }

        public bool ReadBool(string nodeId)
        {
            if (clientSession == null) throw new Exception("Not connected");

            var dv = clientSession.ReadValue(NodeId.Parse(nodeId));
            return Convert.ToBoolean(dv.Value);
        }

        public void WriteValue(string nodeId, object value)
        {
            if (clientSession == null) throw new Exception("Not connected");

            var nid = NodeId.Parse(nodeId);
            var current = clientSession.ReadValue(nid);

            object typedValue = ChangeType(current, value);

            var wv = new WriteValue
            {
                NodeId = nid,
                AttributeId = Attributes.Value,
                Value = new DataValue(new Variant(typedValue))
            };

            clientSession.Write(
                null,
                new WriteValueCollection { wv },
                out var results,
                out _);

            if (StatusCode.IsBad(results[0]))
                throw new Exception($"OPCUA Write failed: {results[0]}");
        }

        private static object ChangeType(DataValue current, object value)
        {
            switch (current.WrappedValue.TypeInfo.BuiltInType)
            {
                case BuiltInType.Boolean: return Convert.ToBoolean(value);
                case BuiltInType.Int16: return Convert.ToInt16(value);
                case BuiltInType.UInt16: return Convert.ToUInt16(value);
                case BuiltInType.Int32: return Convert.ToInt32(value);
                case BuiltInType.UInt32: return Convert.ToUInt32(value);
                case BuiltInType.Float: return Convert.ToSingle(value);
                case BuiltInType.Double: return Convert.ToDouble(value);
                default: return value;
            }
        }
    }

    // Custom OPC UA Server 
    internal class CrossroadOpcUaServer : StandardServer
    {
        private CrossroadNodeManager _nodeManager;

        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, Opc.Ua.ApplicationConfiguration configuration)
        {
            Console.WriteLine("Creating master node manager...");

            var nodeManagers = new List<INodeManager>();
            
            // Note: CoreNodeManager requires dynamicNamespaceIndex parameter
            // We skip it and only use our custom node manager

            // Create our custom node manager
            _nodeManager = new CrossroadNodeManager(server, configuration);
            nodeManagers.Add(_nodeManager);

            return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        }

        protected override ServerProperties LoadServerProperties()
        {
            var properties = new ServerProperties
            {
                ManufacturerName = "JAN0837",
                ProductName = "JAN0837 DP OPC UA Server",
                ProductUri = "http://jan0837/opcuaserver",
                SoftwareVersion = Utils.GetAssemblySoftwareVersion(),
                BuildNumber = Utils.GetAssemblyBuildNumber(),
                BuildDate = Utils.GetAssemblyTimestamp()
            };

            return properties;
        }

        public void UpdateVariable(string variableName, bool value)
        {
            _nodeManager?.UpdateVariable(variableName, value);
        }

        public bool ReadVariable(string variableName)
        {
            return _nodeManager?.ReadVariable(variableName) ?? false;
        }
    }

    // Node Manager for variables
    internal class CrossroadNodeManager : CustomNodeManager2
    {
        private readonly Dictionary<string, BaseDataVariableState> _variables = new Dictionary<string, BaseDataVariableState>();
        private FolderState _crossroadFolder;
        private ushort _namespaceIndex;

        public CrossroadNodeManager(IServerInternal server, Opc.Ua.ApplicationConfiguration configuration)
            : base(server, configuration, Namespaces.CrossroadNamespace)
        {
            SystemContext.NodeIdFactory = this;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                LoadPredefinedNodes(SystemContext, externalReferences);

                // Get namespace index
                _namespaceIndex = Server.NamespaceUris.GetIndexOrAppend(Namespaces.CrossroadNamespace);

                // Create Crossroad folder
                _crossroadFolder = CreateFolder(null, "Crossroad", "Crossroad");
                _crossroadFolder.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);
                AddRootNotifier(_crossroadFolder);

                // Create all variables
                CreateVariable(_crossroadFolder, "CrossroadType", "CrossroadType", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "BtnCrossroadStart", "Start Button", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "BtnCrossroadPause", "Pause Button", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "BtnCrossroadStop", "Stop Button", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "BtnCrosswalk1", "Crosswalk 1 Button", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "BtnCrosswalk2", "Crosswalk 2 Button", DataTypeIds.Boolean, false);

                CreateVariable(_crossroadFolder, "TrafficLight1_Green", "Traffic Light 1 Green", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "TrafficLight1_Yellow", "Traffic Light 1 Yellow", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "TrafficLight1_Red", "Traffic Light 1 Red", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "TrafficLight2_Green", "Traffic Light 2 Green", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "TrafficLight2_Yellow", "Traffic Light 2 Yellow", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "TrafficLight2_Red", "Traffic Light 2 Red", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "Pedestrian1_Green", "Pedestrian 1 Green", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "Pedestrian1_Red", "Pedestrian 1 Red", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "Pedestrian2_Green", "Pedestrian 2 Green", DataTypeIds.Boolean, false);
                CreateVariable(_crossroadFolder, "Pedestrian2_Red", "Pedestrian 2 Red", DataTypeIds.Boolean, false);

                AddPredefinedNode(SystemContext, _crossroadFolder);

                Console.WriteLine($"Created {_variables.Count} OPC UA variables in namespace index {_namespaceIndex}");
            }
        }

        private FolderState CreateFolder(NodeState parent, string path, string name)
        {
            var folder = new FolderState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId(path, _namespaceIndex),
                BrowseName = new QualifiedName(path, _namespaceIndex),
                DisplayName = new Opc.Ua.LocalizedText("en", name),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None
            };

            parent?.AddChild(folder);
            return folder;
        }

        private void CreateVariable(NodeState parent, string path, string name, NodeId dataType, object initialValue)
        {
            var variable = new BaseDataVariableState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                NodeId = new NodeId(path, _namespaceIndex),
                BrowseName = new QualifiedName(path, _namespaceIndex),
                DisplayName = new Opc.Ua.LocalizedText("en", name),
                WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                DataType = dataType,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false,
                Value = initialValue,
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow
            };

            // Allow write access - handle writes in the main loop
            variable.OnWriteValue = null;  // Using default write handling

            parent?.AddChild(variable);
            _variables[path] = variable;
            AddPredefinedNode(SystemContext, variable);
        }

        public void UpdateVariable(string variableName, bool value)
        {
            lock (Lock)
            {
                if (_variables.TryGetValue(variableName, out var variable))
                {
                    variable.Value = value;
                    variable.Timestamp = DateTime.UtcNow;
                    variable.ClearChangeMasks(SystemContext, false);
                }
            }
        }

        public bool ReadVariable(string variableName)
        {
            lock (Lock)
            {
                if (_variables.TryGetValue(variableName, out var variable))
                {
                    return Convert.ToBoolean(variable.Value);
                }
            }
            return false;
        }
    }

    // Namespace definitions
    internal static class Namespaces
    {
        public const string CrossroadNamespace = "http://jan0837.opcua.server/data";
    }

    internal static class OpcUaConfigHelpers
    {
        public static Opc.Ua.ApplicationConfiguration LoadConfiguration(string configPath, Opc.Ua.ApplicationType appType)
        {
            using var stream = File.OpenRead(configPath);
            var serializer = new DataContractSerializer(typeof(Opc.Ua.ApplicationConfiguration));

            if (serializer.ReadObject(stream) is not Opc.Ua.ApplicationConfiguration config)
            {
                throw new InvalidOperationException($"Invalid OPC UA config: {configPath}");
            }

            config.ApplicationType = appType;
            config.Validate(appType);
            return config;
        }

        public static async Task EnsureApplicationCertificateAsync(Opc.Ua.ApplicationConfiguration config)
        {
            var pkiRoot = Path.Combine(Path.GetTempPath(), "OPC Foundation", "pki");
            Directory.CreateDirectory(Path.Combine(pkiRoot, "own"));
            Directory.CreateDirectory(Path.Combine(pkiRoot, "trusted"));
            Directory.CreateDirectory(Path.Combine(pkiRoot, "issuer"));
            Directory.CreateDirectory(Path.Combine(pkiRoot, "rejected"));

            var certIdentifier = config.SecurityConfiguration.ApplicationCertificate;
            var certificate = await certIdentifier.Find(true);

            if (certificate == null)
            {
                certificate = CertificateFactory.CreateCertificate(
                    config.ApplicationUri,
                    config.ApplicationName,
                    "CN=" + config.ApplicationName,
                    null
                ).CreateForRSA();

                config.SecurityConfiguration.ApplicationCertificate.Certificate = certificate;
            }
        }
    }

    public static class OpcUaXmlBoot
    {
        // Example: Start client session using XML config (Client.Config.xml)
        public static async Task<Opc.Ua.Client.Session> StartClientFromXmlAsync(
            string configPath,
            string serverUrl,
            string? username = null,
            string? password = null,
            bool autoAcceptUntrusted = true)
        {
            // Load application configuration from XML file
            var config = OpcUaConfigHelpers.LoadConfiguration(configPath, Opc.Ua.ApplicationType.Client);

            // Optional: auto-accept untrusted certificates (DEV only)
            config.CertificateValidator.CertificateValidation += (s, e) =>
            {
                if (autoAcceptUntrusted) e.Accept = true;
            };

            // Ensure application certificate exists
            await OpcUaConfigHelpers.EnsureApplicationCertificateAsync(config);

            // Select endpoint (secure if available)
            var ep = CoreClientUtils.SelectEndpoint(config, serverUrl, true);
            var endpoint = new ConfiguredEndpoint(null, ep, EndpointConfiguration.Create(config));

            // Identity (Anonymous or Username/Password)
            IUserIdentity identity =
                string.IsNullOrWhiteSpace(username)
                    ? new UserIdentity(new AnonymousIdentityToken())
                    : new UserIdentity(new UserNameIdentityToken
                    {
                        UserName = username!,
                        Password = System.Text.Encoding.UTF8.GetBytes(password ?? string.Empty)
                    });

            //var endpointCollection = new EndpointDescriptionCollection { ep };

            var session = await Opc.Ua.Client.Session.Create(
                config,
                endpoint,
                false,
                config.ApplicationName ?? "XmlClient",
                60000,
                identity,
                null
            );

            return session;
        }

        // Example: Start server using XML config (Server.Config.xml)
        public static async Task<(ApplicationInstance App, StandardServer Server)> StartServerFromXmlAsync(
            string configPath,
            bool autoAcceptUntrusted = true)
        {
            // Load server configuration from XML file
            var config = OpcUaConfigHelpers.LoadConfiguration(configPath, Opc.Ua.ApplicationType.Server);

            // Optional: auto-accept untrusted certs (DEV only)
            config.SecurityConfiguration.AutoAcceptUntrustedCertificates = autoAcceptUntrusted;

            // Ensure application certificate exists
            await OpcUaConfigHelpers.EnsureApplicationCertificateAsync(config);

            // Start server
            var app = new ApplicationInstance
            {
                ApplicationType = Opc.Ua.ApplicationType.Server,
                ApplicationConfiguration = config
            };

            var server = new StandardServer();
            await app.Start(server);

            return (app, server);
        }
    }


}
