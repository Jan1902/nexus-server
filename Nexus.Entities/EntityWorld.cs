using Nexus.Entities.Abstraction;
using Nexus.Shared;

namespace Nexus.Entities;

internal class EntityWorld : DomainEntityBase
{
    private readonly List<EntityBase> _entities = [];

    public void SpawnEntity(EntityBase entity)
    {
        _entities.Add(entity);

        AddDomainEvent(new EntitySpawnedEvent(entity));
    }

    public void DespawnEntityAsync(EntityBase entity) => _entities.Remove(entity);

    public EntityBase? GetEntityById(int entityId)
        => _entities.FirstOrDefault(e => e.EntityID == entityId);

    public IEnumerable<EntityBase> GetAllEntities()
        => _entities;

    public override IEnumerable<DomainEntityBase> GetConnectedEntities()
        => _entities;
}
