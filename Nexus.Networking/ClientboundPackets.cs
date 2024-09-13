using Nexus.Networking.Abstraction;

namespace Nexus.Networking;

public record HandshakeCBPacket(string Username, string Address, int Port) : IPacket;