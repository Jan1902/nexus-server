using MediatR;

namespace Nexus.Framework.Abstraction;

public class NexusMediator
{
    public static IMediator Instance { get; private set; } = null!;

    public NexusMediator(IMediator mediator) => Instance = mediator;
}
