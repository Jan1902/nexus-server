using Autofac;
using Microsoft.Extensions.Configuration;

namespace Nexus.Framework.Abstraction;

public static class ContainerBuilderExtensions
{
    private const string ConfigurationSuffix = "Configuration";

    public static void RegisterConfiguration<T>(this ContainerBuilder builder) where T : class, new()
    {
        var section = new T();
        builder.Register((c) =>
        {
            var config = c.Resolve<IConfiguration>();
            config.GetSection(typeof(T).Name.Replace(ConfigurationSuffix, "")).Bind(section);
            return section;
        }).As<T>();
    }
}
