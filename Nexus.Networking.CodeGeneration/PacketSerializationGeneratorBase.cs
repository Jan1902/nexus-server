using Microsoft.CodeAnalysis;

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

    public static string? MapTypeToReaderWriterMethod(string type) => type switch
    {
        "byte" => "Byte",
        "short" => "Short",
        "ushort" => "UShort",
        "int" => "VarInt",
        "Vector3i" => "Position",
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

    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(() => new PacketSerializationSyntaxReceiver());
}