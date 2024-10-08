﻿namespace Nexus.Networking.Abstraction.Packets;

/// <summary>
/// Represents the type of length for a field.
/// </summary>
public enum LengthType
{
    VarIntPrefix,
    Terminated,
    Fixed
}

/// <summary>
/// Specifies the length attribute for a string parameter in a packet.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class LengthAttribute : Attribute
{
    /// <summary>
    /// Gets the type of length.
    /// </summary>
    public LengthType Type { get; }

    /// <summary>
    /// Gets the fixed length if applicable.
    /// </summary>
    public int? Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LengthAttribute"/> class with the specified length type.
    /// </summary>
    /// <param name="type">The type of string length.</param>
    public LengthAttribute(LengthType type)
        => Type = type;

    /// <summary>
    /// Initializes a new instance of the <see cref="LengthAttribute"/> class with the specified fixed length.
    /// </summary>
    /// <param name="length">The fixed length.</param>
    public LengthAttribute(int length)
    {
        Type = LengthType.Fixed;
        Length = length;
    }
}

/// <summary>
/// Specifies that a packet is automatically serialized.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AutoSerializedPacketAttribute"/> class with the specified packet ID and protocol state.
/// </remarks>
/// <param name="packetId">The packet ID.</param>
/// <param name="state">The protocol state.</param>
[AttributeUsage(AttributeTargets.Class)]
public class AutoSerializedPacketAttribute(int packetId, ProtocolState state = ProtocolState.Play, PacketDirection packetDirection = PacketDirection.ClientBound) : PacketAttribute(packetId, state, packetDirection)
{
}

/// <summary>
/// Specifies that a packet is custom serialized using a specific serializer and packet type.
/// </summary>
/// <typeparam name="TSerializer">The type of the packet serializer.</typeparam>
/// <typeparam name="TPacket">The type of the packet.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CustomSerializedAttribute{TSerializer, TPacket}"/> class with the specified packet ID and protocol state.
/// </remarks>
/// <param name="packetId">The packet ID.</param>
/// <param name="state">The protocol state.</param>
[AttributeUsage(AttributeTargets.Class)]
public class CustomSerializedAttribute<TSerializer, TPacket>(int packetId, ProtocolState state = ProtocolState.Play) : PacketAttribute(packetId, state)
    where TSerializer : IPacketSerializer<TPacket>
    where TPacket : PacketBase
{
}

/// <summary>
/// Specifies the packet ID and protocol state for a packet.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PacketAttribute"/> class with the specified packet ID and protocol state.
/// </remarks>
/// <param name="packetId">The packet ID.</param>
/// <param name="state">The protocol state.</param>
[AttributeUsage(AttributeTargets.Class)]
public class PacketAttribute(int packetId, ProtocolState state = ProtocolState.Play, PacketDirection packetDirection = PacketDirection.ClientBound) : Attribute
{
    /// <summary>
    /// Gets the packet ID.
    /// </summary>
    public int PacketId { get; } = packetId;

    /// <summary>
    /// Gets the protocol state.
    /// </summary>
    public ProtocolState State { get; } = state;

    /// <summary>
    /// Gets the packet direction.
    /// </summary>
    public PacketDirection PacketDirection { get; } = packetDirection;
}

/// <summary>
/// Specifies the conditional attribute for a parameter in a packet.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ConditionalAttribute(ConditionalType type = ConditionalType.PreviousBoolean, string? parameterName = null) : Attribute
{
    /// <summary>
    /// Gets the type of conditional attribute.
    /// </summary>
    public ConditionalType Type { get; } = type;

    /// <summary>
    /// The name of the parameter that the conditional attribute depends on.
    /// </summary>
    public string? ConditionalName { get; }
}

/// <summary>
/// Represents the type of conditional attribute.
/// </summary>
public enum ConditionalType
{
    PreviousBoolean,
    NamedBoolean
}

/// <summary>
/// Specifies the bit field attribute for a parameter in a packet.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class BitFieldAttribute() : Attribute;

/// <summary>
/// Specifies the bit set attribute for a parameter in a packet.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class BitSetAttribute(int length) : Attribute
{
    /// <summary>
    /// Gets the length of the bit set.
    /// </summary>
    public int Length { get; } = length;
}

/// <summary>
/// Specifies the overwrite type attribute for a parameter in a packet.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class OverwriteTypeAttribute(OverwriteType type) : Attribute
{
    /// <summary>
    /// Gets the type of overwrite.
    /// </summary>
    public OverwriteType Type { get; } = type;
}

/// <summary>
/// Represents the type of overwrite.
/// </summary>
public enum OverwriteType
{
    Int,
    UByte,
    Byte
}

/// <summary>
/// Specifies that the parameter is an enum.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class EnumAttribute : Attribute;