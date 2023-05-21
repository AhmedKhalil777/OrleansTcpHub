using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using OrleansTcpHub.Models;
using System.Text;
using OrleansTcpHub.Grains;
using Orleans;
using System.Net.Http;

namespace OrleansTcpHub
{
    public class TCPListenerService : BackgroundService
    {
        private readonly IGrainFactory _client;

        public TCPListenerService(IGrainFactory client)
        {
                _client = client;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TcpListener listener = new (IPAddress.Any, 8899);
            listener.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(stoppingToken);
                // Extract necessary information from TcpClient
                TcpClientInfo tcpClientInfo = new TcpClientInfo
                {
                    Client = client,
                    ClientIpAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(),
                    ClientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port
                };

                // Instantiate your Orleans grain and pass the TcpClientInfo object
                var grain = _client.GetGrain<IDeviceGrain>(Guid.NewGuid().ToString());
                await grain.AcceptTcpClient(tcpClientInfo);
            }
        }

        //private async Task ApplyConnection(TcpClient client, CancellationToken cancellationToken)
        //{
        //    NetworkStream stream = client.GetStream();


        //    while (!cancellationToken.IsCancellationRequested)
        //    {
        //        byte[] data = new byte[1024];
        //        int read = await stream.ReadAsync(data, 0, 1024, cancellationToken);
        //        var dataAscii = Encoding.ASCII.GetString(data);
        //        if (dataAscii.Contains("SerialNumber"))
        //        {
        //            var loginData = JsonSerializer.Deserialize<DeviceLogin>(dataAscii);
        //            var deviceGrain = _client.GetGrain<IDeviceGrain>(loginData?.SerialNumber);
        //            await deviceGrain.Connect(client, cancellationToken);
        //            await stream.WriteAsync(data, 0, read, cancellationToken);
        //            continue;
        //        }

        //    }
        //}
    }
}
