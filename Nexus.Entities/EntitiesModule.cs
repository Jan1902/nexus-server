using Autofac;
using Nexus.Framework.Abstraction;

namespace Nexus.Entities;

public class EntitiesModule : ModuleBase
{
    public override void Load(ContainerBuilder builder) => builder.RegisterType<EntityManager>().AsSelf().SingleInstance();
}
