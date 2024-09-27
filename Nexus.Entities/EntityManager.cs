namespace Nexus.Entities;

internal class EntityManager
{
    private readonly List<EntityWorld> _worlds = [];

    private readonly EntityWorld _defaultWorld;

    public EntityManager()
    {
        var defaultWorld = new EntityWorld();

        _worlds.Add(defaultWorld);
        _defaultWorld = defaultWorld;
    }

    public EntityWorld GetDefaultWorld()
        => _defaultWorld;

    public int GetNextEntityID()
        => _worlds.SelectMany(w => w.GetAllEntities()).Max(e => e.EntityID) + 1;
}
