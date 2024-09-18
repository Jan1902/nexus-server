using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.CustomTypes;
using Nexus.Networking.Data;
using Nexus.Networking.Packets;
using System.Net.Sockets;

namespace Nexus.Networking.Connections;

internal class ClientConnection(
    TcpClient tcpClient,
    PacketManager packetManager,
    ILogger<ClientConnection> logger)
{
    public Guid ClientId { get; } = Guid.NewGuid();

    public string? Username { get; set; }

    private readonly byte[] _buffer = new byte[4096];
    private int _currentLength;

    private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    private ProtocolState _protocolState = ProtocolState.Handshake;

    public SemaphoreSlim ReceiveLock { get; } = new(1);

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        using var networkStream = tcpClient.GetStream();

        while (tcpClient.Connected)
        {
            var bytesRead = await networkStream.ReadAsync(_buffer.AsMemory(_currentLength, _buffer.Length - _currentLength), cancellationToken);
            _currentLength += bytesRead;

            if (bytesRead == 0)
                break;

            await CheckForCompletePacketsAsync(cancellationToken);
        }

        logger.LogInformation("Client {id} disconnected", ClientId);
    }

    private async Task CheckForCompletePacketsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _buffer.Length > 0)
        {
            var stream = _memoryStreamManager.GetStream(_buffer);
            var reader = new MinecraftBinaryReader(stream);
            var packetLength = reader.ReadVarInt();

            if (_currentLength < packetLength)
                break;

            var packetData = reader.ReadBytes(packetLength);

            await ReceiveLock.WaitAsync(cancellationToken);
            packetManager.EnqueuePacket(packetData, ClientId, _protocolState);

            var completeSize = (packetLength + packetLength.ToBytesAsVarInt().Length);
            _currentLength -= completeSize;

            Array.Copy(_buffer, completeSize, _buffer, 0, _currentLength);
        }
    }

    public async Task SendPacketAsync<TPacket>(TPacket packet) where TPacket : PacketBase
    {
        var data = packetManager.SerializePacket(packet);
        await tcpClient.GetStream().WriteAsync(data);
    }

    public void SetProtocolState(ProtocolState state) => _protocolState = state;
}