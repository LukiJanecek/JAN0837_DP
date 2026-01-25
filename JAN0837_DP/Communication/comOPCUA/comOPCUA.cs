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
                Console.WriteLine($"Error starting OPC UA server: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                Console.WriteLine($"Error stopping OPC UA server: {ex.Message}");
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

        public async Task<bool> connectToOPCUAserver(string serverURL, string user, string pass)
        {
            if (connected)
            {
                return true;
            }

            try
            {
                ApplicationInstance application = new ApplicationInstance();
                application.ApplicationType = Opc.Ua.ApplicationType.Client;
                application.ConfigSectionName = "Client";
                await application.LoadApplicationConfiguration(false);
                // Certificate will be auto-created if needed
                var configuration = application.ApplicationConfiguration;
                configuration.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;
                var endpointDescription = CoreClientUtils.SelectEndpoint(configuration, serverURL, true, 15000);
                var endpointConfiguration = EndpointConfiguration.Create(configuration);
                var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
                
                // Create user identity (anonymous if no credentials provided)
                IUserIdentity userIdentity = new UserIdentity(new AnonymousIdentityToken());
                
                clientSession = await Opc.Ua.Client.Session.Create(configuration, endpoint, false, "OPCUA Client", 60000, userIdentity, null);
                connected = clientSession.Connected;
                clientSession.KeepAlive += ClientSession_KeepAlive;
                return true;
            }
            catch (Exception ex)
            {
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
            if (reconnectHandler == null) return;

            // převezmi novou session
            clientSession = (Opc.Ua.Client.Session)reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;

            connected = clientSession?.Connected == true;
        }

        private static void CertificateValidator_CertificateValidation(Opc.Ua.CertificateValidator sender, Opc.Ua.CertificateValidationEventArgs e)
        {
            e.Accept = true;
        }

        public void WriteOPCUAValue(opcuaKlient client, string nodeId, object value)
        {
            try
            {
                var nodeIdParsed = NodeId.Parse(nodeId);
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
                    throw new Exception($"Write failed for {nodeId}: {results.FirstOrDefault()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to {nodeId}: {ex.Message}");
            }
        }

        public bool ReadOPCUABool(opcuaKlient client, string nodeId)
        {
            try
            {
                var nodeIdParsed = NodeId.Parse(nodeId);
                DataValue value = client.clientSession.ReadValue(nodeIdParsed);

                if (value != null && value.Value != null)
                {
                    return Convert.ToBoolean(value.Value);
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from {nodeId}: {ex.Message}");
                return false;
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
}
