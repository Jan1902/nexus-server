using Autofac;
using Nexus.Framework.Abstraction;
using Nexus.Networking.Connections;
using Nexus.Networking.Packets;

namespace Nexus.Networking;

public class NetworkingModule : ModuleBase
{
    public override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ClientConnectionFactory>();
        builder.RegisterType<ConnectionHandler>().SingleInstance().AsSelf().AsImplementedInterfaces();
        builder.RegisterType<PacketManager>().SingleInstance().AsSelf().AsImplementedInterfaces();

        builder.RegisterConfiguration<NetworkingConfiguration>();
    }
}
