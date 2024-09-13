namespace Nexus.Framework.Abstraction;

public interface IInitializeAsync
{
    Task InitializeAsync(CancellationToken cancellationToken);
}

public interface IRunAsync
{
    Task RunAsync(CancellationToken cancellationToken);
}

public interface IShutdownAsync
{
    Task ShutdownAsync(CancellationToken cancellationToken);
}

public interface IInitializeAndRunAsync : IInitializeAsync, IRunAsync;