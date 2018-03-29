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
        private readonly IEnumerable<ServerServiceDefinition> _services;
        private readonly ServiceContext _serviceContext;
        private readonly string _endpointName;
        private Server _server;

        public GrpcCommunicationListener(
          IEnumerable<ServerServiceDefinition> services,
          ServiceContext serviceContext,
          string endpointName)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _serviceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
            _endpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
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
            var serviceEndpoint = _serviceContext.CodePackageActivationContext.GetEndpoint(_endpointName);
            var port = serviceEndpoint.Port;
            var host = FabricRuntime.GetNodeContext().IPAddressOrFQDN;

            try
            {
                _server = new Server
                {
                    Ports = { new ServerPort(host, port, ServerCredentials.Insecure) }
                };
                foreach (var service in _services)
                {
                    _server.Services.Add(service);
                }

                _server.Start();

                return $"http://{host}:{port}";
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
    }
}
