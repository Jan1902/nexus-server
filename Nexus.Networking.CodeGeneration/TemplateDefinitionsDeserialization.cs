using Nexus.Framework.CodeGeneration.Shared;

namespace Nexus.Networking.CodeGeneration;

internal static class TemplateDefinitionsDeserialization
{
    public static CodeTemplate DefaultReadTemplate => CodeTemplate.GetTemplate("Reading", "DefaultRead");
    public static CodeTemplate ModelReadTemplate => CodeTemplate.GetTemplate("Reading", "ModelRead");
    public static CodeTemplate DefaultAssignmentTemplate => CodeTemplate.GetTemplate("Reading", "DefaultAssignment");
    public static CodeTemplate IndexAssignmentTemplate => CodeTemplate.GetTemplate("Reading", "IndexAssignment");
    public static CodeTemplate ConditionalReadTemplate => CodeTemplate.GetTemplate("Reading", "ConditionalRead");
    public static CodeTemplate ArrayReadTemplate => CodeTemplate.GetTemplate("Reading", "ArrayRead");
    public static CodeTemplate LengthPrefixReadTemplate => CodeTemplate.GetTemplate("Reading", "LengthPrefixRead");
    public static CodeTemplate ReturnConstructorTemplate => CodeTemplate.GetTemplate("Reading", "ReturnConstructor");
}
