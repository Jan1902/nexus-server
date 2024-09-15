using Autofac;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Nexus.Framework.Abstraction;
using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.Data;
using System.Collections.Concurrent;
using System.Reflection;

namespace Nexus.Networking.Packets;

internal class PacketManager(
    IMediator mediator,
    ILogger<PacketManager> logger) : IInitializeAndRunAsync
{
    private readonly List<PacketRegistration> _registrations = [];

    private readonly ConcurrentQueue<PacketData> _packetQueue = new();

    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        var packetTypes = new List<Type>();

        logger.LogTrace("Setting up packet handling...");

        packetTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(PacketBase).IsAssignableFrom(x) && !x.IsAbstract).ToList();

        foreach (var packetType in packetTypes)
        {
            var attribute = packetType.GetCustomAttribute<PacketAttribute>();
            if (attribute is null)
            {
                logger.LogWarning("Packet type {packetType} is missing PacketInfoAttribute", packetType);
                continue;
            }

            object? serializer = null;
            if (attribute is AutoSerializedPacketAttribute)
            {
                var serializerType = Type.GetType($"{packetType.Namespace}.{packetType.Name}Serializer, {packetType.Assembly.FullName}");
                if (serializerType is not null)
                    serializer = Activator.CreateInstance(serializerType);
            }
            else if (attribute.GetType().IsClosedTypeOf(typeof(CustomSerializedAttribute<,>)))
            {
                var serializerType = attribute.GetType().GetGenericArguments().LastOrDefault();
                if (serializerType is not null)
                    serializer = Activator.CreateInstance(serializerType);
            }

            if (serializer is null)
            {
                logger.LogWarning("Failed to create serializer for packet {packetType}", packetType);
                continue;
            }

            var registration = new PacketRegistration(attribute.PacketId, attribute.State, packetType, serializer);
            _registrations.Add(registration);
        }

        logger.LogTrace("Done setting up packet handling with {count} packets", _registrations.Count);

        return Task.CompletedTask;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_packetQueue.TryDequeue(out var packetData))
                continue;

            var registration = _registrations.FirstOrDefault(x => x.ID == packetData.ID);
            if (registration is null)
            {
                logger.LogInformation("Tried handling unknown packet {id}", packetData.ID);
                continue;
            }

            var serializerType = typeof(IPacketSerializer<>).MakeGenericType(registration.PacketType);
            using var stream = _memoryStreamManager.GetStream(packetData.Data);
            using var reader = new MinecraftBinaryReader(stream);

            var packet = (PacketBase?) serializerType.GetMethod(nameof(IPacketSerializer<PacketBase>.DeserializePacket))?.Invoke(registration.Serializer, [reader]);
            if (packet is null)
            {
                logger.LogWarning("Failed to deserialize packet {id}", packetData.ID);
                continue;
            }

            var messageType = typeof(PacketReceivedMessage<>).MakeGenericType(registration.PacketType);
            var message = Activator.CreateInstance(messageType, [packet, packetData.ClientId]);
            if (message is null)
            {
                logger.LogWarning("Failed to create message for receival of packet {id}", packetData.ID);
                continue;
            }

            logger.LogTrace("Received packet {packetType} from client {clientId}", registration.PacketType.Name, packetData.ClientId);

            await mediator.Publish(message, cancellationToken);
        }
    }

    public void EnqueuePacket(byte[] data, Guid clientId)
    {
        var stream = new MemoryStream(data);
        var reader = new MinecraftBinaryReader(stream);

        var packetId = reader.ReadVarInt();
        var packetData = reader.ReadBytes(data.Length - (int) reader.Position);

        _packetQueue.Enqueue(new PacketData(packetId, packetData, clientId));
    }

    record PacketRegistration(int ID, ProtocolState ProtocolState, Type PacketType, object Serializer);
    record PacketData(int ID, byte[] Data, Guid ClientId);
}