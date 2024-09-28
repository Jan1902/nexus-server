using MediatR;

namespace Nexus.Entities.Abstraction;

public record EntitySpawnedEvent(EntityBase Entity) : INotification;