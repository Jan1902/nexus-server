using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.Abstraction;

namespace Nexus.Networking.Packets.Status;

[AutoSerializedPacket(0x00, ProtocolState.Status, PacketDirection.ClientBound)]
public record StatusResponse(string Status) : PacketBase;

[AutoSerializedPacket(0x01, ProtocolState.Status, PacketDirection.ClientBound)]
public record PingResponse(long Time) : PacketBase;

public record StatusResponseContent(StatusVersion Version, StatusPlayers Players, StatusDescription Description, string Favicon, bool EnforcesSecureChat);
public record StatusVersion(string Name, int Protocol);
public record StatusPlayers(int Max, int Online, StatusPlayersSample[] Sample);
public record StatusPlayersSample(string Name, Guid Id);
public record StatusDescription(string Text);