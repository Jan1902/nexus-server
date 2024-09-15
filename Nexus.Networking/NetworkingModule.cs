using Autofac;
using Nexus.Framework.Abstraction;
using Nexus.Networking.Connections;

namespace Nexus.Networking;

public class NetworkingModule : ModuleBase
{
    public override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ClientConnectionFactory>();
        builder.RegisterType<ConnectionHandler>().SingleInstance().AsImplementedInterfaces();

        builder.RegisterConfiguration<NetworkingConfiguration>();
    }
}
