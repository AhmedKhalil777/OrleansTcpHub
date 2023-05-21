using Orleans.Runtime;
using OrleansTcpHub.Models;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace OrleansTcpHub.Grains
{
    [GenerateSerializer,Immutable]
    public record TcpClientInfo
    {
        public TcpClient Client { get; set; }
        public string ClientIpAddress { get; set; }
        public int ClientPort { get; set; }
        // Add any other necessary properties
    }

    public class DeviceGrain : IDeviceGrain
    {
        private DeviceInfo _deviceInfo = null!;
        private CancellationToken _cancellationToken = default!;
        private NetworkStream _networkStream = null!;
        public Task<bool> Connect(TcpClient client, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _networkStream = client.GetStream();
            return Task.FromResult(true);
        }

        public async Task AcceptTcpClient(TcpClientInfo tcpClientInfo)
        {
            // Use the extracted information to establish a connection or perform necessary operations
            // For example, create a new TcpClient instance using the extracted IP address and port
            _networkStream = tcpClientInfo.Client.GetStream();
            var res = await RecieveAsync();
            var ascii = Encoding.ASCII.GetString(res).Replace("\0", "");
           
            if (ascii.Contains("SerialNumber"))
            {
                var data = JsonSerializer.Deserialize<DeviceLogin>(ascii);
                _deviceInfo = new DeviceInfo(tcpClientInfo.ClientIpAddress, tcpClientInfo.ClientPort, data.SerialNumber, "", DeviceStatus.Connected);
                await SendAsync(Encoding.ASCII.GetBytes("Connected ......................."));
                return;
            }
            await SendAsync(Encoding.ASCII.GetBytes("Can not login"));
            // Handle the TcpClient object as needed
        }

        public async Task<byte[]> RecieveAsync()
        {
            byte[] data = new byte[1024];
            int read = await _networkStream.ReadAsync(data, 0, 1024, _cancellationToken);
            return data;
        }

        public async Task SendAsync(byte[] message)
        {
            await _networkStream.FlushAsync(_cancellationToken);
        
            await _networkStream.WriteAsync(message, 0, message.Length, _cancellationToken);
        }

        public Task<DeviceInfo> GetState()
        {
            return Task.FromResult(_deviceInfo);
        }
    }

    public interface IDeviceGrain : IGrainWithStringKey
    {
        Task<DeviceInfo> GetState();
       //  Task<bool> Connect(TcpClient client, CancellationToken cancellationToken);

        Task SendAsync(byte[] message);
        Task<byte[]> RecieveAsync();
        Task AcceptTcpClient(TcpClientInfo tcpClientInfo);

    }

    public enum DeviceStatus
    {
        Disconnected,
        Connected
    }


}
