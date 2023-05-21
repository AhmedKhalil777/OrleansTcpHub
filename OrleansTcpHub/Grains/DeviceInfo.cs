

namespace OrleansTcpHub.Grains
{
    [Immutable, GenerateSerializer]
    public record class DeviceInfo(string ClientAddress,int ClientPort ,string SerialNumber, string DeviceIdentifie, DeviceStatus status);
}
