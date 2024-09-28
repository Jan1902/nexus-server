using MediatR;
using Nexus.Entities.Abstraction;
using Nexus.Networking.Abstraction;
using Nexus.Shared;

namespace Nexus.SharedModule;

public class EventHandler(IMediator mediator)
    : INotificationHandler<PlayerJoinedEvent>,
    INotificationHandler<EntitySpawnedEvent>
{
    private readonly List<PlayerJoinedEvent> _spawningPlayers = [];

    public Task Handle(PlayerJoinedEvent playerJoined, CancellationToken cancellationToken)
    {
        _spawningPlayers.Add(playerJoined);

        return Task.CompletedTask;
    }

    public Task Handle(EntitySpawnedEvent entitySpawned, CancellationToken cancellationToken)
    {
        if (entitySpawned.Entity is not Player player)
            return Task.CompletedTask;

        var spawningPlayer = _spawningPlayers.FirstOrDefault(x => x.Username == player.Name)
            ?? throw new InvalidOperationException("Spawned entity for unknown player.");

        _spawningPlayers.Remove(spawningPlayer);

        var loginPlay = new LoginPlay(player.EntityID, false, ["minecraft:overworld"], 20, 100, 50, false, false, false, DimensionType.Overworld, "Default",
            new Random().NextInt64(), player.GameMode, GameMode.Undefined, false, false, false, null, null, 0, false);

        return mediator.Publish(new SendPacketToClientMessage<LoginPlay>(loginPlay, spawningPlayer.ClientId), cancellationToken);
    }
}
