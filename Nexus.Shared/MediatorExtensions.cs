using MediatR;

namespace Nexus.Shared;

public static class MediatorExtensions
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, DomainEntityBase entity)
    {
        var domainEvents = entity.DomainEvents;
        entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);

        foreach (var connectedEntity in entity.GetConnectedEntities())
            await mediator.DispatchDomainEventsAsync(connectedEntity);
    }
}
