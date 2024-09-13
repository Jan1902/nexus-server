using Autofac;
using Microsoft.Extensions.Logging;
using Nexus.Framework.Abstraction;

namespace Nexus.Start;

internal class Server(
    ILogger<Server> logger,
    IComponentContext context)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting server...");

        logger.LogInformation("Initializing services");

        try
        {
            await Task.WhenAll(context.Resolve<IEnumerable<IInitializeAsync>>().Select(i => i.InitializeAsync(cancellationToken)));

            logger.LogInformation("Running services...");

            await Task.WhenAll(context.Resolve<IEnumerable<IRunAsync>>().Select(i => i.RunAsync(cancellationToken)));
        }
        catch (Exception e) when (e is TaskCanceledException or OperationCanceledException)
        {
            logger.LogInformation("Stopping server...");
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while running the server");
        }

        try
        {
            var timeoutTask = Task.Delay(5000);
            var shutdownTask = Task.WhenAll(context.Resolve<IEnumerable<IShutdownAsync>>().Select(i => i.ShutdownAsync(cancellationToken)));

            var firstTask = await Task.WhenAny(shutdownTask, timeoutTask);
            if (firstTask == timeoutTask)
            {
                logger.LogWarning("Server shutdown timed out");
                return;
            }
        }
        catch(Exception e)
        {
            logger.LogError(e, "An error occurred while shutting down the server");
        }
    }
}
