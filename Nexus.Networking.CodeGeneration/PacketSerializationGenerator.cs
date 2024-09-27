using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nexus.Framework.CodeGeneration.Shared;
using System.Text;

namespace Nexus.Networking.CodeGeneration;

[Generator]
public class PacketSerializationGenerator : PacketSerializationGeneratorBase, ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not PacketSerializationSyntaxReceiver receiver)
            return;

        foreach (var packet in receiver.PacketDeclerations)
            context.AddSource($"{packet.Identifier.Text}{FileNameSuffix}", BuildSerializerClass(packet, TemplateDefinitionsShared.PacketSerializerTemplate));

        foreach (var model in receiver.ModelDeclerations)
            context.AddSource($"{model.Identifier.Text}{FileNameSuffix}", BuildSerializerClass(model, TemplateDefinitionsShared.ModelSerializerTemplate));
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
        content = CodeFormatter.FormatCode(content);

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

            if (parameterType is null)
                continue;

            // Arrays
            var isArray = false;
            if (parameterType.EndsWith("[]"))
            {
                isArray = true;
                parameterType = parameterType.Replace("[]", "");
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

            var conditional = false;
            // Conditional values
            if (parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString() == ConditionalAttributeName))
                    || parameterType.EndsWith("?"))
            {
                parameterType = parameterType?.Replace("?", "");
                conditional = true;
            }

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

            CodeTemplate defaultAccess = TemplateDefinitionsSerialization.DefaultAccess
                .Set("field", parameterName)
                .Set("cast", castType is not null ? $"({castType}) " : string.Empty);

            CodeTemplate writeTemplate;
            if (writerMethod is null)
                writeTemplate = TemplateDefinitionsSerialization.ModelWriteTemplate.Set("type", parameterType);
            else
                writeTemplate = TemplateDefinitionsSerialization.DefaultWriteTemplate.Set("writerMethod", writerMethod);

            writeTemplate.Set("writeContent", defaultAccess.Render());

            // Array
            if (isArray)
            {
                writeTemplate.Set("writeContent", "item");

                var template = TemplateDefinitionsSerialization.ArrayWriteTemplate
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

                var template = TemplateDefinitionsSerialization.ConditionalWriteTemplate
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
                readTemplate = TemplateDefinitionsDeserialization.ModelReadTemplate
                    .Set("type", parameterType);
            }
            else
            {
                readTemplate = TemplateDefinitionsDeserialization.DefaultReadTemplate
                    .Set("readerMethod", readerMethod)
                    .Set("cast", isBitField || isEnum ? $"({parameterType}) " : string.Empty);
            }

            // Array
            if (isArray)
            {
                if (fixedLength is null)
                {
                    var lengthPrefixTemplate = TemplateDefinitionsDeserialization.LengthPrefixReadTemplate
                        .Set("variable", parameterName);

                    parameterBuilder.AppendLine(lengthPrefixTemplate.Render());
                }

                var indexAssignmentTemplate = TemplateDefinitionsDeserialization.IndexAssignmentTemplate
                    .Set("variable", parameterName)
                    .Set("readContent", readTemplate.Render());

                var template = TemplateDefinitionsDeserialization.ArrayReadTemplate
                    .Set("length", fixedLength is not null ? fixedLength.ToString() : $"{parameterName}Length")
                    .Set("type", parameterType)
                    .Set("variable", parameterName)
                    .Set("readContent", indexAssignmentTemplate.Render());

                parameterBuilder.AppendLine(template.Render());
            }
            else
            {
                var assignmentTemplate = TemplateDefinitionsDeserialization.DefaultAssignmentTemplate
                    .Set("type", parameterType)
                    .Set("variable", parameterName)
                    .Set("readContent", readTemplate.Render());

                parameterBuilder.AppendLine(assignmentTemplate.Render());
            }

            if (conditional)
            {
                var currentText = parameterBuilder.ToString();
                parameterBuilder.Clear();

                var template = TemplateDefinitionsDeserialization.ConditionalReadTemplate
                    .Set("type", parameterType)
                    .Set("variable", parameterName)
                    .Set("readContent", currentText);

                parameterBuilder.AppendLine(template.Render());
            }

            builder.Append(parameterBuilder);

            parameters.Add(parameterName);
        }

        var returnTemplate = TemplateDefinitionsDeserialization.ReturnConstructorTemplate
            .Set("type", packet.Identifier.Text)
            .Set("parameters", string.Join(", ", parameters));

        builder.Append(returnTemplate.Render());

        return builder.ToString();
    }
}