using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OPCUA
using Opc;
using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua.Buffers;
using Opc.Ua.Export;
using Opc.Ua.Security;

namespace JAN0837_DP.Communication.comOPCUA
{
    public class OPCUAimServer
    {
        /*
    
        private StandardServer server;
        private ApplicationInstance application;
        private NodeManager nodeManager;
        private BaseDataVariableState myVariable;

        public void Start()
        {
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "MyOpcUaServer",
                ApplicationType = ApplicationType.Server,
                SecurityConfiguration = new SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true
                },
                ServerConfiguration = new ServerConfiguration
                {
                    BaseAddresses = new string[] { "opc.tcp://localhost:4840" }
                },
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 }
            };

            application = new ApplicationInstance
            {
                ApplicationConfiguration = config,
                ApplicationType = ApplicationType.Server
            };

            application.CheckApplicationInstanceCertificate(false, 0).Wait();
            server = new StandardServer();
            application.Start(server).Wait();

            nodeManager = new NodeManager(server, "http://my.opcua.namespace");
            server.NodeManager = new INodeManager[] { nodeManager };

            // Vytvoření proměnné MyVariable v Namespace 2
            myVariable = nodeManager.CreateVariable("ns=2;s=MyVariable", "MyVariable", BuiltInType.Int32);
            myVariable.Value = 0;
        }

        public void UpdateVariableValue(int newValue)
        {
            myVariable.Value = newValue;
            myVariable.Timestamp = DateTime.UtcNow;
        }
    }

    public class NodeManager
    {
        public NodeManager(IServerInternal server, string namespaceUri)
        : base(server, new string[] { namespaceUri }) { }

        public BaseDataVariableState CreateVariable(string nodeId, string displayName, BuiltInType type)
        {
            var variable = new BaseDataVariableState(null)
            {
                NodeId = new NodeId(nodeId),
                BrowseName = new QualifiedName(displayName),
                DisplayName = displayName,
                DataType = (uint)type,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite
            };

            AddPredefinedNode(SystemContext, variable);
            return variable;
        }
    }

    */
    }

    public class OPCUAimKlient
    {
        /*
        static Session ConnectToOpcUaServer(string serverUrl)
        {
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "OpcUaSubscriber",
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true
                },
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
            };

            config.Validate(ApplicationType.Client).Wait();
            var endpointDescription = CoreClientUtils.SelectEndpoint(serverUrl, false);
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            return Session.Create(config, endpoint, false, "OPC UA Subscriber", 60000, null, null).Result;
        }

        static void ReadTagValue(Session session, string tagNodeId)
        {
            try
            {
                var nodeId = new NodeId(tagNodeId);
                DataValue value = session.ReadValue(nodeId);
                Console.WriteLine($"📊 [{tagNodeId}] = {value.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Chyba při čtení tagu [{tagNodeId}]: {ex.Message}");
            }
        }

        */
    }
}
