namespace Nexus.Entities.Abstraction;

using Nexus.Shared;
using Nexus.Shared.Registries;

public abstract class EntityBase(int entityId) : DomainEntityBase
{
    public int EntityID { get; } = entityId;

    public abstract EntityType EntityType { get; }
}

public abstract class LivingEntityBase(int maxHealth, int entityId) : EntityBase(entityId)
{
    public int Health { get; private set; } = maxHealth;
    public int MaxHealth { get; } = maxHealth;

    public bool IsDead => Health <= 0;

    public void Damage(int amount)
    {
        if (IsDead)
            return;

        if (amount < 0)
            throw new ArgumentException("Damage amount cannot be negative.", nameof(amount));

        Health -= amount;

        if (Health < 0)
            Health = 0;
    }
}

public class Player(string name, Guid clientId, int entityId, GameMode gameMode) : LivingEntityBase(20, entityId)
{
    public string Name { get; } = name;
    public Guid ClientId { get; } = clientId;
    public GameMode GameMode { get; set; } = gameMode;

    public override EntityType EntityType => EntityType.Player;
}