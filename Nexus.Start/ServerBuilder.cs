using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Microsoft.Extensions.Logging;
using Nexus.Framework.Abstraction;
using Nexus.Networking;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using System.Reflection;

namespace Nexus.Start;

internal class ServerBuilder
{
    private static readonly Type[] Modules = [
            typeof(NetworkingModule)
        ];

    public static Server BuildServer()
    {
        var builder = new ContainerBuilder();

        // Logging
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);

        builder.RegisterSerilog(loggerConfiguration);

        // MediatR
        var mediatRConfiguration = MediatRConfigurationBuilder
            .Create([.. Modules.Select(m => m.Assembly), Assembly.GetExecutingAssembly()])
            .WithAllOpenGenericHandlerTypesRegistered()
            .Build();

        builder.RegisterMediatR(mediatRConfiguration);

        // Internals
        builder.RegisterType<Server>().AsSelf().SingleInstance();

        // Load Modules
        foreach (var module in Modules)
        {
            var instance = Activator.CreateInstance(module);
            instance?.GetType().GetMethod(nameof(ModuleBase.Load))?.Invoke(instance, [builder]);
        }

        var container = builder.Build();

        var logger = container.Resolve<ILogger<ServerBuilder>>();

        logger.LogInformation("Loaded {Count} modules", Modules.Count());
        logger.LogInformation("Done building server");

        return container.Resolve<Server>();
    }
}