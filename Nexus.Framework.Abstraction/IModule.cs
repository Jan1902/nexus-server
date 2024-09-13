using Autofac;

namespace Nexus.Framework.Abstraction;

public abstract class ModuleBase
{
    public abstract void Load(ContainerBuilder builder);

    public virtual Task Run(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
