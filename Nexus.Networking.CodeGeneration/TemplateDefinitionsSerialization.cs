using Nexus.Framework.CodeGeneration.Shared;

internal static class TemplateDefinitionsSerialization
{
    public static CodeTemplate DefaultWriteTemplate => CodeTemplate.GetTemplate("Writing", "DefaultWrite");
    public static CodeTemplate ConditionalWriteTemplate => CodeTemplate.GetTemplate("Writing", "ConditionalWrite");
    public static CodeTemplate ArrayWriteTemplate => CodeTemplate.GetTemplate("Writing", "ArrayWrite");
    public static CodeTemplate ModelWriteTemplate => CodeTemplate.GetTemplate("Writing", "ModelWrite");
    public static CodeTemplate DefaultAccess => CodeTemplate.GetTemplate("Writing", "DefaultAccess");
}