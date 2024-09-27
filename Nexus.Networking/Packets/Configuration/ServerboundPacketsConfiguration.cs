using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Shared;

namespace Nexus.Networking.Packets.Configuration;

[AutoSerializedPacket(0x07, ProtocolState.Configuration, PacketDirection.ServerBound)]
public record ServerboundKnownPacks(KnownPack[] KnownPacks) : PacketBase;

[AutoSerializedPacket(0x02, ProtocolState.Configuration, PacketDirection.ServerBound)]
public record ServerboundPluginMessage(string Channel) : PacketBase;

[AutoSerializedPacket(0x00, ProtocolState.Configuration, PacketDirection.ServerBound)]
public record ClientInformation(string Locale, byte ViewDistance, [Enum] ChatMode ChatMode, bool ChatColors, [BitField] DisplayedSkinParts SkinParts, [Enum] MainHand MainHand, bool EnableTextFiltering, bool AllowServerListings) : PacketBase;

public enum ChatMode : int
{
    Enabled = 0,
    CommandsOnly = 1,
    Hidden = 2
}

[Flags]
public enum DisplayedSkinParts
{
    Cape = 0x01,
    Jacket = 0x02,
    LeftSleeve = 0x04,
    RightSleeve = 0x08,
    LeftPants = 0x10,
    RightPants = 0x20,
    Hat = 0x40
}

[AutoSerializedPacket(0x03, ProtocolState.Configuration, PacketDirection.ServerBound)]
public record AcknowledgeFinishConfiguration : PacketBase;