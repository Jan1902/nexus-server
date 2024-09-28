using Microsoft.CodeAnalysis;
using Nexus.Framework.CodeGeneration.Shared;
using System.Text.Json;

namespace Nexus.SharedModule.CodeGeneration;

[Generator]
public class RegistryGenerator : ISourceGenerator
{
    private static CodeTemplate EnumTemplate => CodeTemplate.GetTemplate("EnumTemplate", "EnumTemplate");
    private static CodeTemplate EnumValueTemplate => CodeTemplate.GetTemplate("EnumTemplate", "EnumValueTemplate");

    public void Execute(GeneratorExecutionContext context)
    {
        var assembly = GetType().Assembly;
        using var resourceStream = assembly.GetManifestResourceStream("Nexus.SharedModule.CodeGeneration.Resources.registries.json");

        if (resourceStream != null)
        {
            using var reader = new StreamReader(resourceStream);
            var jsonText = reader.ReadToEnd();

            var jsonDocument = JsonDocument.Parse(jsonText);

            foreach (var registry in jsonDocument.RootElement.EnumerateObject())
            {
                var registryName = registry.Name;
                registryName = NormalizeName(registryName);
                registryName = ConvertToPascalCase(registryName);

                var entries = registry.Value
                    .GetProperty("entries")
                    .EnumerateObject()
                    .Select(e => (
                        Name: ConvertToPascalCase(NormalizeName(e.Name)),
                        Value: e.Value.GetProperty("protocol_id").GetInt32()));

                var values = entries.OrderBy(e => e.Value).Select(e
                    => EnumValueTemplate
                        .Set("name", e.Name)
                        .Set("value", e.Value.ToString())
                        .Render());

                var template = EnumTemplate
                    .Set("type", registryName)
                    .Set("values", string.Join(",\n", values));

                context.AddSource($"{registryName}.g.cs", CodeFormatter.FormatCode(template.Render()));
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context) { }

    public static string NormalizeName(string name) => name.Replace("minecraft:", "").Replace("/", "_").Replace(":", "_").Replace(".", "_");

    public static string ConvertToPascalCase(string name) => string.Join("", name.Split('_').Select(word => word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower()));
}
