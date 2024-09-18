using Nexus.Framework.CodeGeneration.Shared;

namespace Nexus.Networking.CodeGeneration;

internal static class TemplateDefinitionsShared
{
    public static CodeTemplate PacketSerializerTemplate => CodeTemplate.GetTemplate("PacketSerializer", null);
    public static CodeTemplate ModelSerializerTemplate => CodeTemplate.GetTemplate("ModelSerializer", null);
}
