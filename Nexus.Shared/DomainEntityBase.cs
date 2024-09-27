using MediatR;

namespace Nexus.Shared;

public abstract class DomainEntityBase
{
    private readonly List<INotification> _domainEvents = [];

    public IReadOnlyCollection<INotification> DomainEvents
        => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    public void RemoveDomainEvent(INotification domainEvent)
        => _domainEvents.Remove(domainEvent);

    public virtual IEnumerable<DomainEntityBase> GetConnectedEntities()
        => [];
}