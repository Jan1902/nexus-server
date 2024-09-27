using MediatR;

namespace Nexus.Networking.Abstraction;

public record PlayerJoinedEvent(Guid ClientId, string Username) : INotification;