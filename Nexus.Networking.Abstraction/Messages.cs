using MediatR;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Abstraction;

public record SendPacketToClientMessage<TPacket>(TPacket Packet, Guid ClientId) : INotification
    where TPacket : PacketBase;

public record SendPacketToAllClientsMessage<TPacket>(TPacket Packet, ProtocolState ProtocolState = ProtocolState.Play) : INotification
    where TPacket : PacketBase;