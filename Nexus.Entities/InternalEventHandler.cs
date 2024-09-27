using MediatR;

namespace Nexus.Entities;

internal class InternalEventHandler : INotificationHandler<EntitySpawnedEvent>
{
    public Task Handle(EntitySpawnedEvent entitySpawned, CancellationToken cancellationToken) =>
        // Do something with the entity that was spawned.
        Task.CompletedTask;
}
