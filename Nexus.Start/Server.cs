using Autofac;
using Microsoft.Extensions.Logging;
using Nexus.Framework.Abstraction;

namespace Nexus.Start;

internal class Server(
    ILogger<Server> logger,
    IComponentContext context)
{
    private const int ShutdownTimeoutMS = 5000;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting server...");

        try
        {
            var servicesToInitialize = context.Resolve<IEnumerable<IInitializeAsync>>();
            logger.LogInformation("Initializing {count} services", servicesToInitialize.Count());
            var initializeTasks = servicesToInitialize.Select(i => i.InitializeAsync(cancellationToken)).ToList();

            await Task.WhenAll(initializeTasks);

            var servicesToRun = context.Resolve<IEnumerable<IRunAsync>>();
            logger.LogInformation("Running {count} services", servicesToRun.Count());

            await Parallel.ForEachAsync(servicesToRun, cancellationToken, async (s, c) => await s.RunAsync(c));
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
            var servicesToShutdown = context.Resolve<IEnumerable<IShutdownAsync>>();
            logger.LogInformation("Shutting down {count} services", servicesToShutdown.Count());
            var shutdownTasks = servicesToShutdown.Select(i => i.ShutdownAsync(cancellationToken));

            var shutdownTask = Task.WhenAll(shutdownTasks);
            var timeoutTask = Task.Delay(ShutdownTimeoutMS, CancellationToken.None);

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
