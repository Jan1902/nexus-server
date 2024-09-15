using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nexus.Framework.CodeGeneration.Shared;
using System.Text;

namespace Nexus.Networking.CodeGeneration;

public class SyntaxReceiver : ISyntaxReceiver
{
    private const string AutoSerializedPacketAttributeName = "AutoSerializedPacket";

    public List<RecordDeclarationSyntax> PacketDeclerations { get; } = [];
    public List<RecordDeclarationSyntax> ModelDeclerations { get; } = [];

    private readonly List<RecordDeclarationSyntax> _possibleModels = [];
    private readonly List<string> _foundModels = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is RecordDeclarationSyntax declaration)
        {
            if (declaration.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == AutoSerializedPacketAttributeName)))
            {
                PacketDeclerations.Add(declaration);

                if (declaration.ParameterList is null)
                    return;

                var models = declaration.ParameterList.Parameters
                    .Select(a => a.Type?.ToString() ?? string.Empty)
                    .Where(a => PacketSerializationGenerator.MapTypeToReaderWriterMethod(a) is null);

                _foundModels.AddRange(models.Where(m => !_foundModels.Contains(m)));
            }
            else
            {
                _possibleModels.Add(declaration);
            }
        }

        ModelDeclerations.AddRange(_possibleModels.Where(pm => _foundModels.Any(md => md == pm.Identifier.Text) && !ModelDeclerations.Contains(pm)));
    }
}

[Generator]
public class PacketSerializationGenerator : ISourceGenerator
{
    private const string ConditionalAttributeName = "Conditional";
    private const string BitFieldAttributeName = "BitField";
    private const string BitSetAttributeName = "BitSet";
    private const string OverwriteTypeAttributeName = "OverwriteType";
    private const string LengthAttributeName = "Length";

    private const string FallbackNamespace = "Vortex.Generated";
    private const string FileNameSuffix = "_Serializer.g.cs";

    private CodeTemplate PacketSerializerTemplate => CodeTemplate.GetTemplate("PacketSerializer", null);
    private CodeTemplate ModelSerializerTemplate => CodeTemplate.GetTemplate("ModelSerializer", null);

    private CodeTemplate DefaultWriteTemplate => CodeTemplate.GetTemplate("Writing", "DefaultWrite");
    private CodeTemplate ConditionalWriteTemplate => CodeTemplate.GetTemplate("Writing", "ConditionalWrite");
    private CodeTemplate ArrayWriteTemplate => CodeTemplate.GetTemplate("Writing", "ArrayWrite");
    private CodeTemplate ModelWriteTemplate => CodeTemplate.GetTemplate("Writing", "ModelWrite");

    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        foreach (var packet in receiver.PacketDeclerations)
            context.AddSource($"{packet.Identifier.Text}{FileNameSuffix}", BuildSerializerClass(packet, PacketSerializerTemplate));

        foreach (var model in receiver.ModelDeclerations)
            context.AddSource($"{model.Identifier.Text}{FileNameSuffix}", BuildSerializerClass(model, ModelSerializerTemplate));
    }

    private string BuildSerializerClass(RecordDeclarationSyntax packet, CodeTemplate template)
    {
        var typeName = packet.Identifier.Text;
        var typeNamespace = packet.FirstAncestorOrSelf<CompilationUnitSyntax>()?.Members.OfType<FileScopedNamespaceDeclarationSyntax>()?.FirstOrDefault().Name.ToString() ?? FallbackNamespace;

        template.Set("type", typeName);
        template.Set("namespace", typeNamespace);
        template.Set("serializeContent", BuildSerializeMethod(packet));
        template.Set("deserializeContent", BuildDeserializeMethod(packet));

        var test = template.Render();

        return template.Render();
    }

    private string BuildSerializeMethod(RecordDeclarationSyntax packet)
    {
        var builder = new StringBuilder();

        foreach (var parameter in packet.ParameterList?.Parameters ?? [])
        {
            var parameterBuilder = new StringBuilder();

            // Parameter name and type
            var parameterName = parameter.Identifier.Text;
            var parameterType = parameter.Type?.ToString();
            parameterType = parameterType?.Replace("?", "");

            if (parameterType is null)
                continue;

            // Arrays
            var isArray = false;
            if (parameterType.EndsWith("[]"))
            {
                isArray = true;
                parameterType = parameterType.Substring(0, parameterType.Length - 2);
            }

            // Fixed length for arrays
            var lengthAttribute = parameter.AttributeLists.SelectMany(l => l.Attributes).FirstOrDefault(a => a.Name.ToString() == LengthAttributeName);
            int? fixedLength = null;
            if (lengthAttribute is not null)
            {
                var lengthString = lengthAttribute.ArgumentList?.Arguments.FirstOrDefault()?.ToString();

                if (lengthString is not null)
                    fixedLength = int.Parse(lengthString);
            }

            // Conditional values
            var conditional = parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == ConditionalAttributeName));

            // Binary writer method
            var writerMethod = MapTypeToReaderWriterMethod(parameterType);
            var writerMethodParameters = new List<string>();

            // Type overwrite
            var overwriteTypeAttribute = parameter.AttributeLists.SelectMany(l => l.Attributes).FirstOrDefault(a => a.Name.ToString() == OverwriteTypeAttributeName);
            if (overwriteTypeAttribute is not null)
            {
                var memberAccess = (MemberAccessExpressionSyntax?) overwriteTypeAttribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression;

                if (memberAccess is not null)
                    writerMethod = memberAccess.Name.Identifier.ValueText;
            }

            // Bit sets
            var bitSetAttribute = parameter.AttributeLists.SelectMany(l => l.Attributes).FirstOrDefault(a => a.Name.ToString() == BitSetAttributeName);
            if (bitSetAttribute is not null)
            {
                isArray = false;
                writerMethod = "BitSet";
            }

            // Bit fields
            var isBitField = false;
            if (parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == BitFieldAttributeName)))
            {
                writerMethod = "Byte";
                isBitField = true;
            }

            // Byte arrays
            if (writerMethod == "Byte" && isArray)
            {
                writerMethod = "Bytes";
                isArray = false;
                parameterType = "byte[]";

                if (fixedLength is null)
                    writerMethod = "BytesWithVarIntPrefix";
            }

            CodeTemplate writeTemplate;
            if (writerMethod is null)
            {
                writeTemplate = ModelWriteTemplate
                    .Set("field", parameterName)
                    .Set("type", parameterType);
            }
            else
            {
                writeTemplate = DefaultWriteTemplate
                    .Set("field", parameterName)
                    .Set("writerMethod", writerMethod)
                    .Set("cast", isBitField ? "(byte)" : string.Empty);
            }

            // Array
            if (isArray)
            {
                var template = ArrayWriteTemplate
                    .Set("field", parameterName)
                    .Set("writeContent", writeTemplate.Render());

                parameterBuilder.AppendLine(template.Render());
            }
            else
            {
                parameterBuilder.AppendLine(writeTemplate.Render());
            }

            if (conditional)
            {
                var currentText = parameterBuilder.ToString();
                parameterBuilder.Clear();

                var template = ConditionalWriteTemplate
                    .Set("field", parameterName)
                    .Set("writeContent", currentText);

                parameterBuilder.Append(template.Render());
            }

            builder.Append(parameterBuilder);
        }

        return builder.ToString();
    }

    private string BuildDeserializeMethod(RecordDeclarationSyntax packet)
    {
        return "return null;";
    }

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
}