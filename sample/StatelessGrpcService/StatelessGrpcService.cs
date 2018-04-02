using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Services.Grpc.Communication.Grpc.Runtime;

namespace StatelessGrpcService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessGrpcService : StatelessService
    {
        private class GreeterService : Greeter.GreeterBase
        {
            public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
            {
                ServiceEventSource.Current.Message("Server Received: {0}", request.Name);
                return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
            }
        }

        public StatelessGrpcService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(serviceContext => 
                    new GrpcCommunicationListener(new[] { Greeter.BindService(new GreeterService()) }, serviceContext))
            };
        }
    }
}
