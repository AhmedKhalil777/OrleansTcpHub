// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using OrleansTcpHub.Models;

var cts = new CancellationTokenSource();
Parallel.ForEach(Enumerable.Range(1, 1000), x => {
    Thread.Sleep(x * 2);
    var t = new Thread(async () => await AddTcpClient(cts.Token));
    t.Start();
    Console.WriteLine(x + " Client Started");
});

 
async Task AddTcpClient(CancellationToken cancellationToken) {
    try
    {
        using var client = new TcpClient();

        client.Connect("127.0.0.1", 8899);

        using NetworkStream networkStream = client.GetStream();
        networkStream.ReadTimeout = 200000;

        var message = JsonSerializer.Serialize(new DeviceLogin { SerialNumber = Guid.NewGuid().ToString() });

        if (!cancellationToken.IsCancellationRequested)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await networkStream.WriteAsync(bytes, 0, bytes.Length);
            await networkStream.ReadAsync(bytes, 0, bytes.Length);
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Console.ReadLine();

    }

}
