using Autofac;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Nexus.Networking.Connections;

internal class ClientConnectionFactory(IComponentContext context)
{
    public ClientConnection CreateClientConnection(TcpClient tcpClient)
    {
        return new ClientConnection(tcpClient, context.Resolve<ILogger<ClientConnection>>());
    }
}
