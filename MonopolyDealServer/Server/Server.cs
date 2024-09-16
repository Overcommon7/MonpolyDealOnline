using SimpleTCP;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System.Text;

internal static class Server
{
    static SimpleTcpServer mServer;
    public delegate void DataRecieved(ulong clientID, ClientSendMessages message, byte[] data, Message extra);

    public static event DataRecieved? mOnDataRecieved;
    public static event Action<SimpleTcpServer, TcpClient>? mOnClientDisconnected; 
    public static event Action<SimpleTcpServer, TcpClient>? mOnClientConnected;

    public static IPAddress? Address { get; private set; } = null;

    static Server()
    {
        mServer = new SimpleTcpServer();
    }
    public static void Start(int port = 25565)
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Address = ip.Address;
                        break;
                    }
                }
            }

            if (Address is not null)
                break;
        }

        if (Address is null)
        {
            Console.WriteLine("Could Not Find Active Network Adapter To Start Server");
            return;
        }

        mServer.Start(Address, port);
        mServer.ClientDisconnected += ClientDisconnected;
        mServer.ClientConnected += ClientConnected;
        mServer.DataReceived += DataReceived;
        mServer.AutoTrimStrings = false;
        Console.WriteLine($"Server started on Address: {Address} - Port: {port}");
    }

    public static void BroadcastMessage<T>(ServerSendMessages message, ref T data, int playerNumber) where T : struct
    {
        var byteData = Format.ToData(message, ref data, playerNumber);
        mServer.Broadcast(byteData);
    }

    public static void BroadcastMessage<T>(ServerSendMessages message, ref T data) where T : struct
    {
        var byteData = Format.ToData(message, ref data);
        mServer.Broadcast(byteData);
    }

    public static void BroadcastMessage(ServerSendMessages message, string data, int playerNumber)
    {
        var byteData = Format.ToData(message, data, playerNumber);
        mServer.Broadcast(byteData);
    }

    public static void BroadcastMessage(ServerSendMessages message, byte[] data, int playerNumber)
    {
        var header = Format.CreateHeader(message, playerNumber);
        mServer.Broadcast(Format.CombineByteArrays(header, data));
    }

    public static void BroadcastMessage(ServerSendMessages message, int playerNumber)
    {
        mServer.Broadcast(Format.CreateHeader(message, playerNumber));
    }

    private static void DataReceived(object? sender, Message e)
    {
        ulong id = e.TcpClient.GetID();
        Console.WriteLine($"Data Recieved From: {e.TcpClient.Client.RemoteEndPoint?.ToString()} - {id}");

        var messsage = Format.GetMessageType<ClientSendMessages>(e.Data);
        byte[] data;

        if ((int)messsage < 100)
            data = Format.GetByteDataFromMessage(e.Data);
        else
            data = e.Data;

        mOnDataRecieved?.Invoke(id, messsage, data, e);
    }

    private static void ClientConnected(object? sender, TcpClient e)
    {
        StringBuilder builder = new StringBuilder();
        builder
            .Append(e.GetID())
            .Append('|');

        for (int i = 0; i < PlayerManager.TotalPlayers; ++i)
        {
            PlayerManager.TryGetPlayer(i, out var player);
            builder.Append(player.Number).Append(',').Append(player.ID).Append(',').Append(player.Name);

            if (i + 1 != PlayerManager.TotalPlayers)
                builder.Append('|');
        }    

        var playerData = Format.Encode(builder.ToString());
        e.GetStream().Write(playerData);
        mOnClientConnected?.Invoke(mServer, e);
    }

    private static void ClientDisconnected(object? sender, TcpClient e)
    {
        mOnClientDisconnected?.Invoke(mServer, e);
    }


}

