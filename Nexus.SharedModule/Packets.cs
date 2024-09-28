using Nexus.Networking.Abstraction.Packets;
using Nexus.Shared;

namespace Nexus.SharedModule;

[AutoSerializedPacket(0x2B)]
public record LoginPlay(
    int EntityId,
    bool IsHardcore,
    string[] Dimensions,
    int MaxPlayers,
    int ViewDistance,
    int SimulationDistance,
    bool ReducedDebugInfo,
    bool EnableRespawnScreen,
    bool DoLimitedCrafting,
    [Enum] DimensionType DimensionType,
    string DimensionName,
    long HashedSeed,
    [Enum][OverwriteType(OverwriteType.UByte)] GameMode GameMode,
    [Enum][OverwriteType(OverwriteType.Byte)] GameMode PreviousGameMode,
    bool IsDebug,
    bool IsFlat,
    bool HasDeathLocation,
    [Conditional(ConditionalType.NamedBoolean, nameof(HasDeathLocation))] string? DeathDimensionName,
    [Conditional(ConditionalType.NamedBoolean, nameof(HasDeathLocation))] Vector3i? DeathLocation,
    int PortalCooldown,
    bool EnforcesSecureChat) : PacketBase;