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
    private const string EnumAttributeName = "Enum";

    private const string FallbackNamespace = "Vortex.Generated";
    private const string FileNameSuffix = "_Serializer.g.cs";

    private CodeTemplate PacketSerializerTemplate => CodeTemplate.GetTemplate("PacketSerializer", null);
    private CodeTemplate ModelSerializerTemplate => CodeTemplate.GetTemplate("ModelSerializer", null);

    private CodeTemplate DefaultWriteTemplate => CodeTemplate.GetTemplate("Writing", "DefaultWrite");
    private CodeTemplate ConditionalWriteTemplate => CodeTemplate.GetTemplate("Writing", "ConditionalWrite");
    private CodeTemplate ArrayWriteTemplate => CodeTemplate.GetTemplate("Writing", "ArrayWrite");
    private CodeTemplate ModelWriteTemplate => CodeTemplate.GetTemplate("Writing", "ModelWrite");

    private CodeTemplate DefaultReadTemplate => CodeTemplate.GetTemplate("Reading", "DefaultRead");
    private CodeTemplate ConditionalReadTemplate => CodeTemplate.GetTemplate("Reading", "ConditionalRead");
    private CodeTemplate ArrayReadTemplate => CodeTemplate.GetTemplate("Reading", "ArrayRead");
    private CodeTemplate ModelReadTemplate => CodeTemplate.GetTemplate("Reading", "ModelRead");
    private CodeTemplate LengthPrefixReadTemplate => CodeTemplate.GetTemplate("Reading", "LengthPrefixRead");
    private CodeTemplate ReturnConstructorTemplate => CodeTemplate.GetTemplate("Reading", "ReturnConstructor");

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

        var content = template.Render();
        content = FormatCode(content);

        return content;
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

            string? castType = null;

            // Bit fields
            if (parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == BitFieldAttributeName)))
            {
                writerMethod = "Byte";
                castType = "byte";
            }

            // Enums
            if (parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == EnumAttributeName)))
            {
                writerMethod = "VarInt";
                castType = "int";
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
                    .Set("cast", castType is not null ? $"({castType}) " : string.Empty);
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

                parameterBuilder.AppendLine(template.Render());
            }

            builder.Append(parameterBuilder);
        }

        return builder.ToString();
    }

    private string BuildDeserializeMethod(RecordDeclarationSyntax packet)
    {
        var builder = new StringBuilder();

        var parameters = new List<string>();

        foreach (var parameter in packet.ParameterList?.Parameters ?? [])
        {
            var parameterBuilder = new StringBuilder();

            // Parameter name and type
            var parameterName = parameter.Identifier.Text;
            parameterName = parameterName.Substring(0, 1).ToLower() + parameterName.Substring(1);

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

            // Binary reader method
            var readerMethod = MapTypeToReaderWriterMethod(parameterType);
            var readerMethodParameters = new List<string>();

            // Type overwrite
            var overwriteTypeAttribute = parameter.AttributeLists.SelectMany(l => l.Attributes).FirstOrDefault(a => a.Name.ToString() == OverwriteTypeAttributeName);
            if (overwriteTypeAttribute is not null)
            {
                var memberAccess = (MemberAccessExpressionSyntax?) overwriteTypeAttribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression;

                if (memberAccess is not null)
                    readerMethod = memberAccess.Name.Identifier.ValueText;
            }

            // Bit sets
            var bitSetAttribute = parameter.AttributeLists.SelectMany(l => l.Attributes).FirstOrDefault(a => a.Name.ToString() == BitSetAttributeName);
            if (bitSetAttribute is not null)
            {
                isArray = false;
                readerMethod = "BitSet";
            }

            // Bit fields
            var isBitField = false;
            if (parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == BitFieldAttributeName)))
            {
                readerMethod = "Byte";
                isBitField = true;
            }

            // Enums
            var isEnum = false;
            if (parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == EnumAttributeName)))
            {
                readerMethod = "VarInt";
                isEnum = true;
            }

            // Byte arrays
            if (readerMethod == "Byte" && isArray)
            {
                readerMethod = "Bytes";
                isArray = false;
                parameterType = "byte[]";

                if (fixedLength is null)
                    readerMethod = "BytesWithVarIntPrefix";
            }

            CodeTemplate readTemplate;
            if (readerMethod is null)
            {
                readTemplate = ModelReadTemplate
                    .Set("variable", parameterName)
                    .Set("type", parameterType);
            }
            else
            {
                readTemplate = DefaultReadTemplate
                    .Set("type", parameterType)
                    .Set("variable", parameterName)
                    .Set("readerMethod", readerMethod)
                    .Set("cast", isBitField || isEnum ? $"({parameterType}) " : string.Empty);
            }

            // Array
            if (isArray)
            {
                if (fixedLength is not null)
                {
                    var lengthPrefixTemplate = LengthPrefixReadTemplate
                        .Set("variable", parameterName);

                    parameterBuilder.AppendLine(lengthPrefixTemplate.Render());
                }

                var template = ArrayReadTemplate
                    .Set("length", fixedLength is not null ? fixedLength.ToString() : $"{parameterName}Length")
                    .Set("type", parameterType)
                    .Set("variable", parameterName)
                    .Set("readContent", readTemplate.Render());

                parameterBuilder.AppendLine(template.Render());
            }
            else
            {
                parameterBuilder.AppendLine(readTemplate.Render());
            }

            if (conditional)
            {
                var currentText = parameterBuilder.ToString();
                parameterBuilder.Clear();

                var template = ConditionalReadTemplate
                    .Set("type", parameterType)
                    .Set("variable", parameterName)
                    .Set("readContent", currentText);

                parameterBuilder.AppendLine(template.Render());
            }

            builder.Append(parameterBuilder);

            parameters.Add(parameterName);
        }

        var returnTemplate = ReturnConstructorTemplate
            .Set("type", packet.Identifier.Text)
            .Set("parameters", string.Join(", ", parameters));

        builder.Append(returnTemplate.Render());

        return builder.ToString();
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

    private static string FormatCode(string code)
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
}