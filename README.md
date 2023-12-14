
## Usage example

Server:
```c#
#using EasySocket;

///

private static ClientInfo _lastClient;

///

private static void Main()
{
    var server = new Server("127.0.0.1", 22079);
    Console.WriteLine($"Server started on: {server.tcpListener.LocalEndpoint}");

    server.ClientConnected += Server_ClientConnected;
    server.ClientDisconnected += Server_ClientDisconnected;
    server.MessageReceived += Server_MessageReceived;

    new Thread(() =>
        {
            server.Start();
        })
        { IsBackground = true }.Start();

    while (true)
    {
        var message = Console.ReadLine();

        var data = new Data();
        data.Put("message", message);
        if (_lastClient != null) server.SendMessageToClient(_lastClient, data);
    }
}

private static void Server_ClientConnected(ClientInfo obj)
{
    var clientIp = obj.TcpClient.Client.RemoteEndPoint;
    Console.WriteLine($"Client connected: [{obj.ClientId}:{clientIp}]");
    _lastClient = obj;
}
private static void Server_ClientDisconnected(ClientInfo obj)
{
    var clientIp = obj.TcpClient.Client.RemoteEndPoint;
    Console.WriteLine($"Client disconnected: [{obj.ClientId}:{clientIp}]");
    _lastClient = null;
}
private static void Server_MessageReceived(ClientInfo obj, Data data)
{
    var clientIp = obj.TcpClient.Client.RemoteEndPoint;
    var message = data.ReadString("message");
    Console.WriteLine($"Message from [{obj.ClientId}:{clientIp}]: {message}");
}
```

Client:
```c#
using EasySocket;

///

private static void Main()
{
    var client = new Client();
    client.Connected += Client_Connected;
    client.Disconnected += Client_Disconnected;
    client.MessageReceived += Client_MessageReceived;

    client.Connect("127.0.0.1", 22079);

    new Thread(() =>
        {
            while (client.isListening)
            {
                Thread.Sleep(150);
                client.ListenToServerCoroutine();
            }
        })
        { IsBackground = true }.Start();

    while (true)
    {
        var message = Console.ReadLine();

        var data = new Data();
        data.Put("message", message);
        client.SendMessageToServer(data);
    }
}
private static void Client_Connected()
{
    Console.WriteLine("Connected to server.");
}
private static void Client_Disconnected()
{
    Console.WriteLine("Disconnected from server.");
}
private static void Client_MessageReceived(Data data)
{
    var message = data.ReadString("message");
    Console.WriteLine($"Message from [SERVER]: {message}");
}
```
