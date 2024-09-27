using MediatR;
using Microsoft.Extensions.Logging;
using Nexus.Entities.Abstraction;
using Nexus.Networking.Abstraction;
using Nexus.Shared;

namespace Nexus.Entities;

internal class EventHandler(
    EntityManager manager,
    IMediator mediator,
    ILogger<EventHandler> logger) : INotificationHandler<PlayerJoinedEvent>
{
    public async Task Handle(PlayerJoinedEvent playerJoined, CancellationToken cancellationToken)
    {
        var playerEntity = new Player(playerJoined.Username, playerJoined.ClientId, manager.GetNextEntityID());
        var world = manager.GetDefaultWorld();

        world.SpawnEntity(playerEntity);

        await mediator.DispatchDomainEventsAsync(world);

        logger.LogTrace("Spawning entity {entityId} for player {username}", playerEntity.EntityID, playerEntity.Name);
    }
}
