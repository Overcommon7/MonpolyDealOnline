using SimpleTCP;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
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
        Console.WriteLine($"[SERVER] ");

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

    public static void SendMessageToPlayers(ServerSendMessages message, int sentFromPlayerNumber, byte[] data, params int[] playerNumbers)
    {
        var header = Format.CreateHeader(message, sentFromPlayerNumber);
        var byteData = Format.CombineByteArrays(header, data);

        foreach (var playerNumber in playerNumbers)
        {
            var status = PlayerManager.TryGetPlayer(playerNumber, out var player);
            if (status != ConnectionStatus.Connected)
                continue;

            player.Client.GetStream().Write(byteData);
        }
    }

    public static void SendMessageToPlayers<T>(ServerSendMessages message, int sentFromPlayerNumber, ref T data, params int[] playerNumbers) where T : struct
    {
        var byteData = Format.ToData(message, ref data, sentFromPlayerNumber);

        foreach (var playerNumber in playerNumbers)
        {
            var status = PlayerManager.TryGetPlayer(playerNumber, out var player);
            if (status != ConnectionStatus.Connected)
                continue;

            player.Client.GetStream().Write(byteData);
        }
    }

    public static void SendMessageToPlayers(ServerSendMessages message, int sentFromPlayerNumber, byte[] data, params TcpClient[] players)
    {
        var header = Format.CreateHeader(message, sentFromPlayerNumber);
        var byteData = Format.CombineByteArrays(header, data);

        foreach (var player in players)
        {
            player.GetStream().Write(byteData);
        }
    }

    public static void SendMessageToPlayers<T>(ServerSendMessages message, int sentFromPlayerNumber, ref T data, params TcpClient[] players) where T : struct
    {
        var byteData = Format.ToData(message, ref data, sentFromPlayerNumber);

        foreach (var player in players)
        {
            player.GetStream().Write(byteData);
        }
    }

    private static void DataReceived(object? sender, Message e)
    {
        ulong id = e.TcpClient.GetID();
        var messsage = Format.GetMessageType<ClientSendMessages>(e.Data);
        if (PlayerManager.TryGetPlayer(id, out var player) != ConnectionStatus.Invalid)
            Console.WriteLine($"[Server] R: {player.Name} - Type: {messsage} - #: {player.Number}");

        byte[] data;

        if ((int)messsage < Format.HEADER_SIZE)
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
            var status = PlayerManager.TryGetPlayer(i, out var player);
            if (status != ConnectionStatus.Connected)
                continue;

            builder.Append(player.Number).Append(',').Append(player.ID).Append(',').Append(player.Name);

            if (i + 1 != PlayerManager.TotalPlayers)
                builder.Append('|');
        }

        var playerData = Format.ToData(ServerSendMessages.OnPlayerIDAssigned, builder.ToString(), PlayerManager.TotalPlayers + 1);
        e.GetStream().Write(playerData);

        mOnClientConnected?.Invoke(mServer, e);
    }

    private static void ClientDisconnected(object? sender, TcpClient e)
    {
        mOnClientDisconnected?.Invoke(mServer, e);
    }


}

