using MediatR;
using Nexus.Networking.Abstraction;

namespace Nexus.SharedModule;

public class EventHandler(IMediator mediator) : INotificationHandler<PlayerJoinedEvent>
{
    public Task Handle(PlayerJoinedEvent notification, CancellationToken cancellationToken) =>
        //var loginPlay = new LoginPlay();

        //return mediator.Publish(new SendPacketToClientMessage<LoginPlay>(loginPlay, notification.ClientId), cancellationToken);

        Task.CompletedTask;
}
