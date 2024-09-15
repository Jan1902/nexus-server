namespace Nexus.Networking.Abstraction;

public enum ProtocolState : int
{
    Handshake = 0,
    Status = 1,
    Login = 2,
    Configuration = 3,
    Play = 4
}