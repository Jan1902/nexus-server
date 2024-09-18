using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nexus.Networking.CodeGeneration;

public class PacketSerializationSyntaxReceiver : ISyntaxReceiver
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
                    .Select(a => a.Type?.ToString().Replace("[]", "").Replace("?", "") ?? string.Empty)
                    .Where(a => PacketSerializationGeneratorBase.MapTypeToReaderWriterMethod(a) is null);

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
