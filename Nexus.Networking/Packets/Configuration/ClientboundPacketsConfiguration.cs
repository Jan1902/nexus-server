using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets.Configuration;

[AutoSerializedPacket(0x0E, ProtocolState.Configuration)]
public record ClientboundKnownPacks(KnownPack[] KnownPacks) : PacketBase;

[AutoSerializedPacket(0x07, ProtocolState.Configuration)]
public record RegistryData(string RegistryId, RegistryEntry[] Entries) : PacketBase;

public record RegistryEntry(string Identifier, NbtTag? Data);

[AutoSerializedPacket(0x03, ProtocolState.Configuration, PacketDirection.ClientBound)]
public record FinishConfiguration() : PacketBase;