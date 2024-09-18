namespace Nexus.Networking;

public class NetworkingConfiguration
{
    public int Port { get; set; } = 25565;
    public int MaxConnections { get; set; } = 20;
    public int ProtocolVersion { get; set; } = 767;
    public string ServerVersion { get; set; } = "1.21.1";
    public string Motd { get; set; } = "Nexus - A Minecraft server thought different";
    public int KeepAliveIntervalSeconds { get; set; } = 10;
}
