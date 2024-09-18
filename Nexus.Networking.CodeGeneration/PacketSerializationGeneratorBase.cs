using Microsoft.CodeAnalysis;
using System.Text;

namespace Nexus.Networking.CodeGeneration;

[Generator]
public class PacketSerializationGeneratorBase
{
    protected const string BitFieldAttributeName = "BitField";
    protected const string BitSetAttributeName = "BitSet";
    protected const string ConditionalAttributeName = "Conditional";
    protected const string EnumAttributeName = "Enum";

    protected const string FallbackNamespace = "Vortex.Generated";
    protected const string FileNameSuffix = "_Serializer.g.cs";
    protected const string LengthAttributeName = "Length";
    protected const string OverwriteTypeAttributeName = "OverwriteType";

    public static string? MapTypeToReaderWriterMethod(string type)
    {
        return type switch
        {
            "byte" => "Byte",
            "short" => "Short",
            "ushort" => "UShort",
            "int" => "VarInt",
            "uint" => "UInt",
            "long" => "Long",
            "ulong" => "ULong",
            "float" => "Float",
            "double" => "Double",
            "bool" => "Bool",
            "string" => "StringWithVarIntPrefix",
            "Guid" => "UUID",
            "NbtTag" => "NbtTag",
            _ => null
        };
    }

    protected static string FormatCode(string code)
    {
        var lines = code.Split('\n').Select(s => s.Trim());

        var strBuilder = new StringBuilder();

        int indentCount = 0;
        bool shouldIndent = false;

        foreach (string line in lines)
        {
            if (shouldIndent)
                indentCount++;

            if (line.Trim() == "}")
                indentCount--;

            if (indentCount == 0)
            {
                strBuilder.AppendLine(line);
                shouldIndent = line.Contains("{");

                continue;
            }

            string blankSpace = string.Empty;
            for (int i = 0; i < indentCount; i++)
            {
                blankSpace += "    ";
            }

            if (line.Contains("}") && line.Trim() != "}")
                indentCount--;

            strBuilder.AppendLine(blankSpace + line);
            shouldIndent = line.Contains("{");
        }

        return strBuilder.ToString();
    }

    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(() => new PacketSerializationSyntaxReceiver());
}