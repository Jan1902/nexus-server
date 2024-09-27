using MediatR;
using Nexus.Entities.Abstraction;

namespace Nexus.Entities;

public record EntitySpawnedEvent(EntityBase Entity) : INotification;