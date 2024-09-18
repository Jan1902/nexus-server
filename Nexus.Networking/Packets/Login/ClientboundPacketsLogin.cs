using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets.Login;

[AutoSerializedPacket(0x02, Abstraction.ProtocolState.Login)]
public record LoginSuccess(Guid UUID, string Username, Property[] Properties, bool StrictErrorHandling) : PacketBase;

public record Property(string Name, string Value, string? Signature);