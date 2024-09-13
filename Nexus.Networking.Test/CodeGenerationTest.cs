using Nexus.Networking.CodeGeneration;

namespace Nexus.Networking.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var generator = new PacketSerializationGenerator();
        var context = new Microsoft.CodeAnalysis.GeneratorExecutionContext();
        generator.Execute(context);
        Console.WriteLine();
    }
}
