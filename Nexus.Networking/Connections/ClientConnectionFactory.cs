using Autofac;
using Microsoft.Extensions.Logging;
using Nexus.Networking.Packets;
using System.Net.Sockets;

namespace Nexus.Networking.Connections;

internal class ClientConnectionFactory(IComponentContext context)
{
    public ClientConnection CreateClientConnection(TcpClient tcpClient) => new(
            tcpClient,
            context.Resolve<PacketManager>(),
            context.Resolve<ILogger<ClientConnection>>());
}
