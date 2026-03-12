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
using System.Globalization;
using JAN0837_DP.Data;
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
        public void UpdateBoolVariable(string variableName, bool value)
        {
            if (_server != null && running)
            {
                _server.UpdateBoolVariable(variableName, value);
            }
        }

        public void UpdateStringVariable(string variableName, string value)
        {
            if (_server != null && running)
            {
                _server.UpdateStringVariable(variableName, value);
            }
        }

        public void UpdateIntVariable(string variableName, int value)
        {
            if (_server != null && running)
            {
                _server.UpdateIntVariable(variableName, value);
            }
        }

        public void UpdateFloatVariable(string variableName, float value)
        {
            if (_server != null && running)
            {
                _server.UpdateFloatVariable(variableName, value);
            }
        }

        public void UpdateDoubleVariable(string variableName, double value)
        {
            if (_server != null && running)
            {
                _server.UpdateDoubleVariable(variableName, value);
            }
        }

        public void UpdateRealVariable(string variableName, double value)
        {
            if (_server != null && running)
            {
                _server.UpdateDoubleVariable(variableName, value);
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

        public string ReadVariableAsString(string variableName)
        {
            if (_server != null && running)
            {
                var raw = _server.ReadVariableRaw(variableName);
                return raw?.ToString() ?? string.Empty;
            }
            return string.Empty;
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
                        Logger.LogError("No endpoint found that supports UserName authentication. Check server configuration.");
                        throw new Exception("No endpoint found that supports UserName authentication. Check server configuration.");
                    }
                }
                else
                {
                    // Anonymous - use simple endpoint selection (no security required)
                    endpointDescription = await Opc.Ua.Client.CoreClientUtils.SelectEndpointAsync(config, serverURL, false, 15000);
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
                clientSession = await Opc.Ua.Client.Session.Create(config, endpoint, false, "OPCUA Client Session", 60000, userIdentity, null);

                connected = clientSession.Connected;
                clientSession.KeepAliveInterval = 5000;
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
                // Dispose reconnect handler first
                var handler = reconnectHandler;
                reconnectHandler = null;
                handler?.Dispose();

                // Capture session reference locally to avoid race condition
                var session = clientSession;
                clientSession = null;
                connected = false;

                if (session != null)
                {
                    try
                    {
                        session.KeepAlive -= ClientSession_KeepAlive;
                    }
                    catch { /* Ignore if already unsubscribed */ }

                    try
                    {
                        if (session.Connected)
                            await session.CloseAsync();
                    }
                    catch { /* Ignore close errors */ }

                    try
                    {
                        session.Dispose();
                    }
                    catch { /* Ignore dispose errors */ }
                }

                Logger.LogInfo("OPC UA client disconnected successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OPC UA Disconnect");
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
            reconnectHandler.dispose();
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
                var oldSession = clientSession;
                clientSession = null;
                if (oldSession != null)
                {
                    try
                    {
                        oldSession.KeepAlive -= ClientSession_KeepAlive;
                        if (oldSession.Connected)
                            await oldSession.CloseAsync();
                        oldSession.Dispose();
                    }
                    catch { /* Ignore cleanup errors */ }
                }

                connected = false;

                // Attempt reconnection
                bool success = await connectToOPCUAserver(_lastServerUrl, _lastUser, _lastPass);

                if (success)
                {
                    Console.WriteLine("Reconnection successfull!");
                    Logger.LogInfo("OPCUA reconnection successfull.");
                }
                else
                {
                    Console.WriteLine("Reconnection failed.");
                    Logger.LogError("OPCUA reconnection failed.");
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

                // Capture session reference locally to prevent race condition
                var session = client.clientSession;
                if (session == null)
                {
                    Console.WriteLine($"Session became null, cannot write to {nodeId}");
                    connected = false;
                    return;
                }

                var nodeIdParsed = NodeId.Parse(nodeId);

                // Optional: Pre-read to check node status (useful for debugging)
                session.Read(null, 0, TimestampsToReturn.Neither,
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
                    Logger.LogError($"Warning: Node {nodeId} read status before write: {readValues[0].StatusCode}");
                }

                var wv = new WriteValue
                {
                    NodeId = nodeIdParsed,
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value))
                };

                var valuesToWrite = new WriteValueCollection { wv };
                session.Write(null, valuesToWrite, out StatusCodeCollection results, out _);

                if (results.Count != 1 || StatusCode.IsBad(results[0]))
                {
                    Console.WriteLine($"Write failed for {nodeId}: 0x{results[0].Code:X8}");
                    Logger.LogError($"Write failed for {nodeId}: 0x{results[0].Code:X8}");
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
                    ex.StatusCode == StatusCodes.BadConnectionClosed ||
                    ex.StatusCode == StatusCodes.BadNotConnected ||
                    ex.StatusCode == StatusCodes.BadServerNotConnected)
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
                    Logger.LogWarning($"Session invalid, cannot read from {nodeId}");
                    return false;
                }

                // Capture session reference locally to prevent race condition
                var session = client.clientSession;
                if (session == null)
                {
                    Logger.LogWarning($"Session became null, cannot read from {nodeId}");
                    connected = false;
                    return false;
                }

                var id = NodeId.Parse(nodeId);

                // Read value directly (single read instead of double)
                DataValue value = session.ReadValue(id);

                if (StatusCode.IsBad(value.StatusCode))
                {
                    Logger.LogWarning($"Read {nodeId} returned bad status: 0x{value.StatusCode.Code:X8}");
                    return false;
                }

                if (value.Value != null)
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
                    ex.StatusCode == StatusCodes.BadConnectionClosed ||
                    ex.StatusCode == StatusCodes.BadNotConnected ||
                    ex.StatusCode == StatusCodes.BadServerNotConnected)
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

        public string ReadOPCUAInt(opcuaKlient client, string nodeId)
        {
            try
            {
                // Validate session before reading
                if (!ValidateSession())
                {
                    Logger.LogWarning($"Session invalid, cannot read from {nodeId}");
                    return string.Empty;
                }

                // Capture session reference locally to prevent race condition
                var session = client.clientSession;
                if (session == null)
                {
                    Logger.LogWarning($"Session became null, cannot read from {nodeId}");
                    connected = false;
                    return string.Empty;
                }

                var id = NodeId.Parse(nodeId);

                // Read value directly (single read instead of double)
                DataValue value = session.ReadValue(id);

                if (StatusCode.IsBad(value.StatusCode))
                {
                    Logger.LogWarning($"Read {nodeId} returned bad status: 0x{value.StatusCode.Code:X8}");
                    return string.Empty;
                }

                if (value.Value != null)
                {
                    return Convert.ToString(value.Value) ?? string.Empty;
                }

                return string.Empty;
            }
            catch (ServiceResultException ex)
            {
                // Handle session-related errors with reconnection
                if (ex.StatusCode == StatusCodes.BadSessionIdInvalid ||
                    ex.StatusCode == StatusCodes.BadSessionClosed ||
                    ex.StatusCode == StatusCodes.BadSessionNotActivated ||
                    ex.StatusCode == StatusCodes.BadSecureChannelClosed ||
                    ex.StatusCode == StatusCodes.BadConnectionClosed ||
                    ex.StatusCode == StatusCodes.BadNotConnected ||
                    ex.StatusCode == StatusCodes.BadServerNotConnected)
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
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"OPC UA ReadInt {nodeId}");
                return string.Empty;
            }
        }

        public float ReadOPCUAFloat(opcuaKlient client, string nodeId)
        {
            try
            {
                // Validate session before reading
                if (!ValidateSession())
                {
                    Logger.LogWarning($"Session invalid, cannot read from {nodeId}");
                    return 0f;
                }

                // Capture session reference locally to prevent race condition
                var session = client.clientSession;
                if (session == null)
                {
                    Logger.LogWarning($"Session became null, cannot read from {nodeId}");
                    connected = false;
                    return 0f;
                }

                var id = NodeId.Parse(nodeId);

                // Read value directly (single read instead of double)
                DataValue value = session.ReadValue(id);

                if (StatusCode.IsBad(value.StatusCode))
                {
                    Logger.LogWarning($"Read {nodeId} returned bad status: 0x{value.StatusCode.Code:X8}");
                    return 0f;
                }

                if (value.Value != null)
                {
                    return Convert.ToSingle(value.Value);
                }

                return 0f;
            }
            catch (ServiceResultException ex)
            {
                // Handle session-related errors with reconnection
                if (ex.StatusCode == StatusCodes.BadSessionIdInvalid ||
                    ex.StatusCode == StatusCodes.BadSessionClosed ||
                    ex.StatusCode == StatusCodes.BadSessionNotActivated ||
                    ex.StatusCode == StatusCodes.BadSecureChannelClosed ||
                    ex.StatusCode == StatusCodes.BadConnectionClosed ||
                    ex.StatusCode == StatusCodes.BadNotConnected ||
                    ex.StatusCode == StatusCodes.BadServerNotConnected)
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
                return 0f;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"OPC UA ReadFloat {nodeId}");
                return 0f;
            }
        }

        public double ReadOPCUADouble(opcuaKlient client, string nodeId)
        {
            try
            {
                // Validate session before reading
                if (!ValidateSession())
                {
                    Logger.LogWarning($"Session invalid, cannot read from {nodeId}");
                    return 0d;
                }

                // Capture session reference locally to prevent race condition
                var session = client.clientSession;
                if (session == null)
                {
                    Logger.LogWarning($"Session became null, cannot read from {nodeId}");
                    connected = false;
                    return 0d;
                }

                var id = NodeId.Parse(nodeId);

                // Read value directly (single read instead of double)
                DataValue value = session.ReadValue(id);

                if (StatusCode.IsBad(value.StatusCode))
                {
                    Logger.LogWarning($"Read {nodeId} returned bad status: 0x{value.StatusCode.Code:X8}");
                    return 0d;
                }

                if (value.Value != null)
                {
                    return Convert.ToDouble(value.Value);
                }

                return 0d;
            }
            catch (ServiceResultException ex)
            {
                // Handle session-related errors with reconnection
                if (ex.StatusCode == StatusCodes.BadSessionIdInvalid ||
                    ex.StatusCode == StatusCodes.BadSessionClosed ||
                    ex.StatusCode == StatusCodes.BadSessionNotActivated ||
                    ex.StatusCode == StatusCodes.BadSecureChannelClosed ||
                    ex.StatusCode == StatusCodes.BadConnectionClosed ||
                    ex.StatusCode == StatusCodes.BadNotConnected ||
                    ex.StatusCode == StatusCodes.BadServerNotConnected)
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
                return 0d;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"OPC UA ReadDouble {nodeId}");
                return 0d;
            }
        }

        public string ReadOPCUAString(opcuaKlient client, string nodeId)
        {
            try
            {
                // Validate session before reading
                if (!ValidateSession())
                {
                    Logger.LogWarning($"Session invalid, cannot read from {nodeId}");
                    return string.Empty;
                }

                // Capture session reference locally to prevent race condition
                var session = client.clientSession;
                if (session == null)
                {
                    Logger.LogWarning($"Session became null, cannot read from {nodeId}");
                    connected = false;
                    return string.Empty;
                }

                var id = NodeId.Parse(nodeId);

                // Read value directly (single read instead of double)
                DataValue value = session.ReadValue(id);

                if (StatusCode.IsBad(value.StatusCode))
                {
                    Logger.LogWarning($"Read {nodeId} returned bad status: 0x{value.StatusCode.Code:X8}");
                    return string.Empty;
                }

                if (value.Value != null)
                {
                    return Convert.ToString(value.Value) ?? string.Empty;
                }

                return string.Empty;
            }
            catch (ServiceResultException ex)
            {
                // Handle session-related errors with reconnection
                if (ex.StatusCode == StatusCodes.BadSessionIdInvalid ||
                    ex.StatusCode == StatusCodes.BadSessionClosed ||
                    ex.StatusCode == StatusCodes.BadSessionNotActivated ||
                    ex.StatusCode == StatusCodes.BadSecureChannelClosed ||
                    ex.StatusCode == StatusCodes.BadConnectionClosed ||
                    ex.StatusCode == StatusCodes.BadNotConnected ||
                    ex.StatusCode == StatusCodes.BadServerNotConnected)
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
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"OPC UA ReadString {nodeId}");
                return string.Empty;
            }
        }

        // Validate and restore session if needed
        private bool ValidateSession()
        {
            var session = clientSession;
            if (session == null)
            {
                return false;
            }

            if (!session.Connected)
            {
                connected = false;
                return false;
            }

            // Check if KeepAlive is stopped and restart it
            if (session.KeepAliveStopped)
            {
                Console.WriteLine("KeepAlive stopped, restarting...");
                try
                {
                    // Restart keep-alive by setting the interval again
                    session.KeepAlive += ClientSession_KeepAlive;
                    session.KeepAliveInterval = 5000;

                    // The session will automatically start sending keep-alives
                    Console.WriteLine("KeepAlive restarted successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restart KeepAlive: {ex.Message}");
                    Logger.LogException(ex, "OPC UA KeepAlive Restart");
                    connected = false;
                    return false;
                }
            }

            return true;
        }

        private void HandleSessionError(ServiceResultException ex, string context)
        {
            if (ex.StatusCode == StatusCodes.BadSessionIdInvalid ||
                ex.StatusCode == StatusCodes.BadSessionClosed ||
                ex.StatusCode == StatusCodes.BadSessionNotActivated ||
                ex.StatusCode == StatusCodes.BadSecureChannelClosed ||
                ex.StatusCode == StatusCodes.BadConnectionClosed ||
                ex.StatusCode == StatusCodes.BadNotConnected ||
                ex.StatusCode == StatusCodes.BadServerNotConnected)
            {
                Logger.LogError($"Session error in {context}: [0x{ex.StatusCode:X8}] {ex.Message}");
                connected = false;
                if (!_isReconnecting)
                {
                    _ = TryReconnectAsync();
                }
            }
            else
            {
                Logger.LogError($"OPC UA error in {context}: [0x{ex.StatusCode:X8}] {ex.Message}");
            }
        }

        public bool BulkWrite(List<(string nodeId, object value)> items)
        {
            try
            {
                var session = clientSession;
                if (session == null || !session.Connected)
                {
                    connected = false;
                    return false;
                }

                var writeValues = new WriteValueCollection();
                foreach (var (nodeId, value) in items)
                {
                    writeValues.Add(new WriteValue
                    {
                        NodeId = NodeId.Parse(nodeId),
                        AttributeId = Attributes.Value,
                        Value = new DataValue(new Variant(value))
                    });
                }

                session.Write(null, writeValues, out StatusCodeCollection results, out _);

                bool allOk = true;
                for (int i = 0; i < results.Count; i++)
                {
                    if (StatusCode.IsBad(results[i]))
                    {
                        Logger.LogError($"BulkWrite failed for {items[i].nodeId}: 0x{results[i].Code:X8}");
                        allOk = false;
                    }
                }
                return allOk;
            }
            catch (ServiceResultException ex)
            {
                HandleSessionError(ex, "BulkWrite");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OPC UA BulkWrite");
                return false;
            }
        }

        public DataValueCollection BulkRead(string[] nodeIds)
        {
            try
            {
                var session = clientSession;
                if (session == null || !session.Connected)
                {
                    connected = false;
                    return null;
                }

                var readValueIds = new ReadValueIdCollection();
                foreach (var nodeId in nodeIds)
                {
                    readValueIds.Add(new ReadValueId
                    {
                        NodeId = NodeId.Parse(nodeId),
                        AttributeId = Attributes.Value
                    });
                }

                session.Read(null, 0, TimestampsToReturn.Neither, readValueIds, out DataValueCollection results, out _);
                return results;
            }
            catch (ServiceResultException ex)
            {
                HandleSessionError(ex, "BulkRead");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OPC UA BulkRead");
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // Bulk helpers for extracting values from DataValueCollection
        // ═══════════════════════════════════════════════════════════════════

        private static bool GetBoolResult(DataValueCollection results, int index)
        {
            if (index < results.Count && StatusCode.IsGood(results[index].StatusCode) && results[index].Value != null)
                return Convert.ToBoolean(results[index].Value);
            return false;
        }

        private static float GetFloatResult(DataValueCollection results, int index)
        {
            if (index < results.Count && StatusCode.IsGood(results[index].StatusCode) && results[index].Value != null)
                return Convert.ToSingle(results[index].Value);
            return 0f;
        }

        private static float ParseFloat(string s)
            => float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0f;

        // ═══════════════════════════════════════════════════════════════════
        // BulkReadAllOutputs – hromadné čtení všech výstupů z PLC (1x OPC UA Read)
        // ═══════════════════════════════════════════════════════════════════

        public bool BulkReadAllOutputs()
        {
            string[] readNodeIds =
            [
                // CrossroadData outputs (index 0–20)
                CrossroadData.OpcUaNodeIds.crossroadType,               // 0
                CrossroadData.OpcUaNodeIds.trafficLightNorth_green,      // 1
                CrossroadData.OpcUaNodeIds.trafficLightNorth_yellow,     // 2
                CrossroadData.OpcUaNodeIds.trafficLightNorth_red,        // 3
                CrossroadData.OpcUaNodeIds.trafficLightSouth_green,      // 4
                CrossroadData.OpcUaNodeIds.trafficLightSouth_yellow,     // 5
                CrossroadData.OpcUaNodeIds.trafficLightSouth_red,        // 6
                CrossroadData.OpcUaNodeIds.trafficLightEast_green,       // 7
                CrossroadData.OpcUaNodeIds.trafficLightEast_yellow,      // 8
                CrossroadData.OpcUaNodeIds.trafficLightEast_red,         // 9
                CrossroadData.OpcUaNodeIds.trafficLightWest_green,       // 10
                CrossroadData.OpcUaNodeIds.trafficLightWest_yellow,      // 11
                CrossroadData.OpcUaNodeIds.trafficLightWest_red,         // 12
                CrossroadData.OpcUaNodeIds.pedestrianSouth1_green,       // 13
                CrossroadData.OpcUaNodeIds.pedestrianSouth1_red,         // 14
                CrossroadData.OpcUaNodeIds.pedestrianSouth2_green,       // 15
                CrossroadData.OpcUaNodeIds.pedestrianSouth2_red,         // 16
                CrossroadData.OpcUaNodeIds.pedestrianWest1_green,        // 17
                CrossroadData.OpcUaNodeIds.pedestrianWest1_red,          // 18
                CrossroadData.OpcUaNodeIds.pedestrianWest2_green,        // 19
                CrossroadData.OpcUaNodeIds.pedestrianWest2_red,          // 20

                // CrosswalkData outputs (index 21–31)
                CrosswalkData.OpcUaNodeIds.crosswalkType,                // 21
                CrosswalkData.OpcUaNodeIds.trafficLight1_green,          // 22
                CrosswalkData.OpcUaNodeIds.trafficLight1_yellow,         // 23
                CrosswalkData.OpcUaNodeIds.trafficLight1_red,            // 24
                CrosswalkData.OpcUaNodeIds.trafficLight2_green,          // 25
                CrosswalkData.OpcUaNodeIds.trafficLight2_yellow,         // 26
                CrosswalkData.OpcUaNodeIds.trafficLight2_red,            // 27
                CrosswalkData.OpcUaNodeIds.pedestrian1_green,            // 28
                CrosswalkData.OpcUaNodeIds.pedestrian1_red,              // 29
                CrosswalkData.OpcUaNodeIds.pedestrian2_green,            // 30
                CrosswalkData.OpcUaNodeIds.pedestrian2_red,              // 31

                // RegulatorData output (index 32)
                RegulatorData.OpcUaNodeIds.Uin,                          // 32

                // CarLightData outputs (index 33–37)
                CarLightData.OpcUaNodeIds.btnReset,                      // 33
                CarLightData.OpcUaNodeIds.lowBeamLight,                  // 34
                CarLightData.OpcUaNodeIds.highBeamLight,                 // 35
                CarLightData.OpcUaNodeIds.turnLight,                     // 36
                CarLightData.OpcUaNodeIds.result                         // 37
            ];

            var results = BulkRead(readNodeIds);
            if (results == null)
                return false;

            // CrossroadData outputs (index 0–20)
            CrossroadData.crossroadType           = GetBoolResult(results, 0)  ? "true" : "false";
            CrossroadData.trafficLightNorth_green = GetBoolResult(results, 1)  ? "true" : "false";
            CrossroadData.trafficLightNorth_yellow= GetBoolResult(results, 2)  ? "true" : "false";
            CrossroadData.trafficLightNorth_red   = GetBoolResult(results, 3)  ? "true" : "false";
            CrossroadData.trafficLightSouth_green = GetBoolResult(results, 4)  ? "true" : "false";
            CrossroadData.trafficLightSouth_yellow= GetBoolResult(results, 5)  ? "true" : "false";
            CrossroadData.trafficLightSouth_red   = GetBoolResult(results, 6)  ? "true" : "false";
            CrossroadData.trafficLightEast_green  = GetBoolResult(results, 7)  ? "true" : "false";
            CrossroadData.trafficLightEast_yellow = GetBoolResult(results, 8)  ? "true" : "false";
            CrossroadData.trafficLightEast_red    = GetBoolResult(results, 9)  ? "true" : "false";
            CrossroadData.trafficLightWest_green  = GetBoolResult(results, 10) ? "true" : "false";
            CrossroadData.trafficLightWest_yellow = GetBoolResult(results, 11) ? "true" : "false";
            CrossroadData.trafficLightWest_red    = GetBoolResult(results, 12) ? "true" : "false";
            CrossroadData.pedestrianSouth1_green  = GetBoolResult(results, 13) ? "true" : "false";
            CrossroadData.pedestrianSouth1_red    = GetBoolResult(results, 14) ? "true" : "false";
            CrossroadData.pedestrianSouth2_green  = GetBoolResult(results, 15) ? "true" : "false";
            CrossroadData.pedestrianSouth2_red    = GetBoolResult(results, 16) ? "true" : "false";
            CrossroadData.pedestrianWest1_green   = GetBoolResult(results, 17) ? "true" : "false";
            CrossroadData.pedestrianWest1_red     = GetBoolResult(results, 18) ? "true" : "false";
            CrossroadData.pedestrianWest2_green   = GetBoolResult(results, 19) ? "true" : "false";
            CrossroadData.pedestrianWest2_red     = GetBoolResult(results, 20) ? "true" : "false";

            // CrosswalkData outputs (index 21–31)
            CrosswalkData.crosswalkType           = GetBoolResult(results, 21) ? "true" : "false";
            CrosswalkData.trafficLight1_green     = GetBoolResult(results, 22) ? "true" : "false";
            CrosswalkData.trafficLight1_yellow    = GetBoolResult(results, 23) ? "true" : "false";
            CrosswalkData.trafficLight1_red       = GetBoolResult(results, 24) ? "true" : "false";
            CrosswalkData.trafficLight2_green     = GetBoolResult(results, 25) ? "true" : "false";
            CrosswalkData.trafficLight2_yellow    = GetBoolResult(results, 26) ? "true" : "false";
            CrosswalkData.trafficLight2_red       = GetBoolResult(results, 27) ? "true" : "false";
            CrosswalkData.pedestrian1_green       = GetBoolResult(results, 28) ? "true" : "false";
            CrosswalkData.pedestrian1_red         = GetBoolResult(results, 29) ? "true" : "false";
            CrosswalkData.pedestrian2_green       = GetBoolResult(results, 30) ? "true" : "false";
            CrosswalkData.pedestrian2_red         = GetBoolResult(results, 31) ? "true" : "false";

            // RegulatorData output (index 32)
            RegulatorData.Uin = GetFloatResult(results, 32).ToString(CultureInfo.InvariantCulture);

            // CarLightData outputs (index 33–37)
            CarLightData.btnReset       = GetBoolResult(results, 33) ? "true" : "false";
            CarLightData.lowBeamLight   = GetBoolResult(results, 34) ? "true" : "false";
            CarLightData.highBeamLight  = GetBoolResult(results, 35) ? "true" : "false";
            CarLightData.turnLight      = GetBoolResult(results, 36) ? "true" : "false";
            CarLightData.result         = GetBoolResult(results, 37) ? "true" : "false";

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // BulkWriteAllInputs – hromadný zápis všech vstupů do PLC (1x OPC UA Write)
        // ═══════════════════════════════════════════════════════════════════

        public bool BulkWriteAllInputs()
        {
            var writeItems = new List<(string nodeId, object value)>
            {
                // CrossroadData inputs
                (CrossroadData.OpcUaNodeIds.btnStart,           CrossroadData.btnStart == "true"),
                (CrossroadData.OpcUaNodeIds.btnPause,           CrossroadData.btnPause == "true"),
                (CrossroadData.OpcUaNodeIds.btnStop,            CrossroadData.btnStop == "true"),
                (CrossroadData.OpcUaNodeIds.btnWestCrosswalk1,  CrossroadData.btnWestCrosswalk1 == "true"),
                (CrossroadData.OpcUaNodeIds.btnWestCrosswalk2,  CrossroadData.btnWestCrosswalk2 == "true"),
                (CrossroadData.OpcUaNodeIds.btnSouthCrosswalk1, CrossroadData.btnSouthCrosswalk1 == "true"),
                (CrossroadData.OpcUaNodeIds.btnSouthCrosswalk2, CrossroadData.btnSouthCrosswalk2 == "true"),

                // CrosswalkData inputs
                (CrosswalkData.OpcUaNodeIds.btnStart,      CrosswalkData.btnStart == "true"),
                (CrosswalkData.OpcUaNodeIds.btnPause,      CrosswalkData.btnPause == "true"),
                (CrosswalkData.OpcUaNodeIds.btnStop,       CrosswalkData.btnStop == "true"),
                (CrosswalkData.OpcUaNodeIds.btnCrosswalk1, CrosswalkData.btnCrosswalk1 == "true"),
                (CrosswalkData.OpcUaNodeIds.btnCrosswalk2, CrosswalkData.btnCrosswalk2 == "true"),

                // RegulatorData inputs
                (RegulatorData.OpcUaNodeIds.btnReset,    RegulatorData.btnReset == "true"),
                (RegulatorData.OpcUaNodeIds.switchstate, RegulatorData.switchstate == "true"),
                (RegulatorData.OpcUaNodeIds.order,       (short)(int.TryParse(RegulatorData.order, out var ordVal) ? ordVal : 0)),
                (RegulatorData.OpcUaNodeIds.R1,  ParseFloat(RegulatorData.R1)),
                (RegulatorData.OpcUaNodeIds.R2,  ParseFloat(RegulatorData.R2)),
                (RegulatorData.OpcUaNodeIds.C1,  ParseFloat(RegulatorData.C1)),
                (RegulatorData.OpcUaNodeIds.C2,  ParseFloat(RegulatorData.C2)),
                (RegulatorData.OpcUaNodeIds.Uc1, ParseFloat(RegulatorData.Uc1)),
                (RegulatorData.OpcUaNodeIds.Uc2, ParseFloat(RegulatorData.Uc2)),
                (RegulatorData.OpcUaNodeIds.Td,  ParseFloat(RegulatorData.Td)),
                (RegulatorData.OpcUaNodeIds.Ts,  ParseFloat(RegulatorData.Ts)),

                // CarLightData inputs
                (CarLightData.OpcUaNodeIds.btnReset,                CarLightData.btnReset == "true"),
                (CarLightData.OpcUaNodeIds.error,                   CarLightData.error == "true"),
                (CarLightData.OpcUaNodeIds.sensorLight,             CarLightData.sensorLight == "true"),
                (CarLightData.OpcUaNodeIds.sensorConnectorConnected, CarLightData.sensorConnectorConnected == "true")
            };

            return BulkWrite(writeItems);
        }

        ///

        public bool ReadBool(string nodeId)
        {
            var session = clientSession;
            if (session == null) throw new Exception("Not connected");

            var dv = session.ReadValue(NodeId.Parse(nodeId));

            return Convert.ToBoolean(dv.Value);
        }

        public void WriteValue(string nodeId, object value)
        {
            var session = clientSession;
            if (session == null) throw new Exception("Not connected");

            var nid = NodeId.Parse(nodeId);
            var current = session.ReadValue(nid);

            object typedValue = ChangeType(current, value);

            var wv = new WriteValue
            {
                NodeId = nid,
                AttributeId = Attributes.Value,
                Value = new DataValue(new Variant(typedValue))
            };

            session.Write(null, new WriteValueCollection { wv }, out var results, out _);

            if (StatusCode.IsBad(results[0]))
            {
                Logger.LogError($"OPCUA Write failed for {nodeId}: 0x{results[0].Code:X8}");
                throw new Exception($"OPCUA Write failed: {results[0]}");
            }
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
    public class CrossroadOpcUaServer : StandardServer
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

        public void UpdateBoolVariable(string variableName, bool value)
        {
            _nodeManager?.UpdateBoolVariable(variableName, value);
        }

        public void UpdateStringVariable(string variableName, string value)
        {
            _nodeManager?.UpdateStringVariable(variableName, value);
        }

        public void UpdateIntVariable(string variableName, int value)
        {
            _nodeManager?.UpdateIntVariable(variableName, value);
        }

        public void UpdateFloatVariable(string variableName, float value)
        {
            _nodeManager?.UpdateFloatVariable(variableName, value);
        }

        public void UpdateDoubleVariable(string variableName, double value)
        {
            _nodeManager?.UpdateDoubleVariable(variableName, value);
        }

        public bool ReadVariable(string variableName)
        {
            return _nodeManager?.ReadVariable(variableName) ?? false;
        }

        public object ReadVariableRaw(string variableName)
        {
            return _nodeManager?.ReadVariableRaw(variableName);
        }
    }

    // Node Manager for variables
    public class CrossroadNodeManager : CustomNodeManager2
    {
        private readonly Dictionary<string, BaseDataVariableState> _variables = new Dictionary<string, BaseDataVariableState>();
        private FolderState _crossroadFolder;
        private ushort _namespaceIndex;

        public CrossroadNodeManager(IServerInternal server, Opc.Ua.ApplicationConfiguration configuration) : base(server, "JAN0837_DP_OPC_UA_Server", "http://jan0837.opcua.server/data")
        {
            SystemContext.NodeIdFactory = this;
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

        public void UpdateBoolVariable(string variableName, bool value)
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

        public void UpdateStringVariable(string variableName, string value)
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

        public void UpdateIntVariable(string variableName, int value)
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

        public void UpdateFloatVariable(string variableName, float value)
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

        public void UpdateDoubleVariable(string variableName, double value)
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

        public object ReadVariableRaw(string variableName)
        {
            lock (Lock)
            {
                if (_variables.TryGetValue(variableName, out var variable))
                {
                    return variable.Value;
                }
            }
            return null;
        }
    }
}
