using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace ServiceFabric.Services.Grpc.Communication.Grpc.Runtime
{
    public class GrpcCommunicationListener : ICommunicationListener
    {
        private const string ServiceEndpointName = "ServiceEndpoint";

        private readonly IEnumerable<ServerServiceDefinition> _services;
        private readonly ServiceContext _serviceContext;
        private readonly ServerPort _serverPort;

        private Server _server;

        public GrpcCommunicationListener(
          IEnumerable<ServerServiceDefinition> services,
          ServiceContext serviceContext)
            : this(services, serviceContext, ServiceEndpointName)
        {
        }

        public GrpcCommunicationListener(
          IEnumerable<ServerServiceDefinition> services,
          ServiceContext serviceContext,
          string endpointName)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _serviceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
            _serverPort = GetServerPort(endpointName);
        }

        public void Abort()
        {
            StopServerAsync().Wait();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return StopServerAsync();
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            try
            {
                _server = new Server
                {
                    Ports = { _serverPort }
                };
                foreach (var service in _services)
                {
                    _server.Services.Add(service);
                }

                _server.Start();

                return $"http://{_serverPort.Host}:{_serverPort.Port}";
            }
            catch (Exception)
            {
                await StopServerAsync();
                throw;
            }
        }

        private async Task StopServerAsync()
        {
            try
            {
                await _server?.ShutdownAsync();
            }
            catch (Exception)
            {
                // no-op
            }
        }

        private ServerPort GetServerPort(string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            var serviceEndpoint = _serviceContext.CodePackageActivationContext.GetEndpoint(endpointName);
            var port = serviceEndpoint.Port;
            var host = _serviceContext.NodeContext.IPAddressOrFQDN;

            return new ServerPort(host, port, ServerCredentials.Insecure);
        }
    }
}
