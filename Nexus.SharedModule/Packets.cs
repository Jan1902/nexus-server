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
    int DimensionType,
    string DimensionName,
    long HashedSeed,
    [Enum] GameMode GameMode,
    byte PreviousGameMode,
    bool IsDebug,
    bool IsFlat,
    bool HasDeathLocation,
    string DeathDimensionName,
    Vector3i DeathLocation,
    int PortalCooldown,
    bool EnforcesSecureChat) : PacketBase;