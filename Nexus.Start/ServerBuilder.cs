using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.Entities;
using Nexus.Framework.Abstraction;
using Nexus.Networking;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using System.Reflection;

namespace Nexus.Start;

internal class ServerBuilder
{
    private static readonly Type[] Modules = [
            typeof(NetworkingModule),
            typeof(SharedModule.SharedModule),
            typeof(EntitiesModule),
        ];

    public static Server BuildServer()
    {
        var builder = new ContainerBuilder();

        // Logging
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Console().MinimumLevel.Verbose()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);

        builder.RegisterSerilog(loggerConfiguration);

        // Configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        builder.RegisterInstance(configuration).As<IConfiguration>();

        // MediatR
        var mediatRConfiguration = MediatRConfigurationBuilder
            .Create([.. Modules.Select(m => m.Assembly), Assembly.GetExecutingAssembly()])
            .WithAllOpenGenericHandlerTypesRegistered()
            .Build();

        builder.RegisterMediatR(mediatRConfiguration);

        // Internals
        builder.RegisterType<Server>().AsSelf().SingleInstance();
        builder.RegisterType<NexusMediator>().AsSelf().SingleInstance();

        // Load Modules
        foreach (var module in Modules)
        {
            var instance = Activator.CreateInstance(module);
            instance?.GetType().GetMethod(nameof(ModuleBase.Load))?.Invoke(instance, [builder]);
        }

        var container = builder.Build();

        var logger = container.Resolve<ILogger<ServerBuilder>>();

        logger.LogInformation("Done building server with {count} modules", Modules.Length);

        return container.Resolve<Server>();
    }
}