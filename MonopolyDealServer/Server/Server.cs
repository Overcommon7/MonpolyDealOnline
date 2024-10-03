using SimpleTCP;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;

struct ClientRequest
{
    public ulong mPlayerID;
    public int mPlayerNumber;
    public ClientSendMessages mMessage;
    public byte[] mData;
}
internal static class Server
{
    static SimpleTcpServer mServer;
    public delegate void DataRecieved(ulong clientID, int playerNumber, ClientSendMessages message, byte[] data);

    public static event DataRecieved? mOnDataRecieved;
    public static event Action<SimpleTcpServer, TcpClient>? mOnClientDisconnected; 
    public static event Action<SimpleTcpServer, TcpClient>? mOnClientConnected;
    private static List<ClientRequest> mRequests;
    private static bool mProcessingRequests = false;
    private static bool mProcessingProfilePicture = false;

    public static IPAddress? Address { get; private set; } = null;

    static Server()
    {
        mRequests = new List<ClientRequest>();
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

        mServer.Delimiter = Format.DELIMITER;
        mServer.Start(Address, port);
        mServer.ClientDisconnected += ClientDisconnected;
        mServer.ClientConnected += ClientConnected;
        mServer.DataReceived += ProfilePictureLogic;
        mServer.DelimiterDataReceived += DataReceived;
        mServer.AutoTrimStrings = false;
        Console.WriteLine($"Server started on Address: {Address} - Port: {port}");
    }

    public static void GameStarted()
    {
        mProcessingProfilePicture = false;
        mServer.DataReceived -= ProfilePictureLogic;
    }

    public static void Close()
    {
        foreach (var player in PlayerManager.ConnectedPlayers)
        {
            player.CloseClient();
        }

        mServer.Stop();
    }

    public static void BroadcastMessage<T>(ServerSendMessages message, ref T data, int playerNumber) where T : struct
    {
        Console.WriteLine($"[SERVER] S: {message} - #{playerNumber}");

        var byteData = Format.ToData(message, ref data, playerNumber);
        mServer.Broadcast(byteData);
    }

    public static void BroadcastMessage<T>(ServerSendMessages message, ref T data) where T : struct
    {
        Console.WriteLine($"[SERVER] S: {message}");

        var byteData = Format.ToData(message, ref data);
        mServer.Broadcast(byteData);
    }

    public static void BroadcastMessage(ServerSendMessages message, string data, int playerNumber)
    {
        Console.WriteLine($"[SERVER] S: {message} - #{playerNumber}");

        var byteData = Format.ToData(message, data, playerNumber);
        mServer.Broadcast(byteData);
    }

    public static void BroadcastMessage(ServerSendMessages message, byte[] data, int playerNumber)
    {
        Console.WriteLine($"[SERVER] S: {message} - #{playerNumber}");

        var header = Format.CreateHeader(message, playerNumber);
        mServer.Broadcast(Format.CombineByteArrays(header, data, true));
    }

    public static void BroadcastMessage(ServerSendMessages message, int playerNumber)
    {
        Console.WriteLine($"[SERVER] S: {message} - #{playerNumber}");
        mServer.Broadcast(Format.CreateHeader(message, playerNumber).AddDelimiter());
    }

    public static void SendMessageExcluding(ServerSendMessages message, int sentFromPlayerNumber, byte[] data, int excludedPlayer)
    {
        var header = Format.CreateHeader(message, sentFromPlayerNumber);
        var byteData = Format.CombineByteArrays(header, data, true);

        foreach (var player in PlayerManager.ConnectedPlayers)
        {
            if (player.Number == excludedPlayer)
                continue;

            player.Client.GetStream().Write(byteData, 0, byteData.Length);
        }
    }

    public static void SendMessageToPlayers(ServerSendMessages message, int sentFromPlayerNumber, byte[] data, params int[] playerNumbers)
    {
        var header = Format.CreateHeader(message, sentFromPlayerNumber);
        var byteData = Format.CombineByteArrays(header, data, true);

        foreach (var playerNumber in playerNumbers)
        {
            var status = PlayerManager.TryGetPlayer(playerNumber, out var player);
            if (status != ConnectionStatus.Connected)
                continue;

            player.Client.GetStream().Write(byteData, 0, byteData.Length);
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

            player.Client.GetStream().Write(byteData, 0, byteData.Length);
        }
    }

    public static void SendMessageToPlayers(ServerSendMessages message, int sentFromPlayerNumber, byte[] data, params TcpClient[] players)
    {
        var header = Format.CreateHeader(message, sentFromPlayerNumber);
        var byteData = Format.CombineByteArrays(header, data, true);

        foreach (var player in players)
        {
            player.GetStream().Write(byteData, 0, byteData.Length);
        }
    }

    public static void SendMessageToPlayers<T>(ServerSendMessages message, int sentFromPlayerNumber, ref T data, params TcpClient[] players) where T : struct
    {
        var byteData = Format.ToData(message, ref data, sentFromPlayerNumber);

        foreach (var player in players)
        {
            player.GetStream().Write(byteData, 0, byteData.Length);
        }
    }

    public static void ProcessClientRequests()
    {
        mProcessingRequests = true;

        foreach (var request in mRequests)
        {
            if (PlayerManager.TryGetPlayer(request.mPlayerID, out var player) != ConnectionStatus.Invalid)
                Console.WriteLine($"[SERVER] R: {player.Name} - Type: {request.mMessage} - #: {player.Number}");

            mOnDataRecieved?.Invoke(request.mPlayerID, request.mPlayerNumber, request.mMessage, request.mData);
        }

        mRequests.Clear();
        mProcessingRequests = false;
    }

    private static void ProfilePictureLogic(object? sender, Message e)
    {
        

        if (!mProcessingProfilePicture)
            return;

        ClientRequest clientRequest = new();
        clientRequest.mPlayerID = e.TcpClient.GetID();
        clientRequest.mMessage = ClientSendMessages.ProfilePictureSent;
        clientRequest.mData = Format.GetByteDataFromMessage(e.Data);
        clientRequest.mPlayerNumber = Format.GetPlayerNumber(e.Data);

        if (mProcessingRequests)
        {
            Task.Run(() =>
            {
                while (mProcessingRequests)
                    Thread.Sleep(33);

            }).Wait();
        }

        lock (mRequests)
            mRequests.Add(clientRequest);

        mProcessingProfilePicture = false;
    }

    private static void DataReceived(object? sender, Message e)
    {
        ClientRequest clientRequest = new();
        clientRequest.mPlayerID = e.TcpClient.GetID();

        if (mProcessingProfilePicture)
        {
            if (!Format.ContainsProperlyFormattedHeader<ClientSendMessages>(e.Data))
                return;

            Task.Run(() =>
            {
                while (mProcessingProfilePicture)
                    Thread.Sleep(10);

            }).Wait();
        }

        if (e.Data.Length >= Format.HEADER_SIZE)
        {
            clientRequest.mMessage = Format.GetMessageType<ClientSendMessages>(e.Data);
            clientRequest.mData = Format.GetByteDataFromMessage(e.Data);
            clientRequest.mPlayerNumber = Format.GetPlayerNumber(e.Data);

            if (clientRequest.mMessage == ClientSendMessages.PingRequested)
            {
                mOnDataRecieved?.Invoke(clientRequest.mPlayerID, clientRequest.mPlayerNumber, clientRequest.mMessage, clientRequest.mData);
                return;
            }

            if (clientRequest.mMessage == ClientSendMessages.ProfilePictureSent)
            {                
                mProcessingProfilePicture = true;
                return;
            }
        }            
        else
        {
            clientRequest.mData = e.Data;
        }
            

        if (mProcessingRequests)
        {
            Task.Run(() =>
            {
                while (mProcessingRequests)
                    Thread.Sleep(10);

            }).Wait();
        }

        lock (mRequests)
            mRequests.Add(clientRequest);
    }

    private static void ClientConnected(object? sender, TcpClient e)
    {
        ulong id = e.GetID();
        var status = PlayerManager.TryGetPlayer(id, out var player);
        if (status == ConnectionStatus.Connected)
            return;

        StringBuilder builder = new StringBuilder();
        builder
            .Append(id)
            .Append('|');

        for (int i = 0; i < PlayerManager.TotalPlayers; ++i)
        {
            status = PlayerManager.TryGetPlayer(i + 1, out player);
            if (status != ConnectionStatus.Connected)
                continue;

            builder
                .Append(player.Number)
                .Append(',')
                .Append(player.ID)
                .Append(',')
                .Append(player.Name)
                .Append('|');
        }

        string data = builder.ToString();
        var playerData = Format.ToData(ServerSendMessages.OnPlayerIDAssigned, data, PlayerManager.TotalPlayers + 1);
        e.GetStream().Write(playerData, 0, playerData.Length);

        mOnClientConnected?.Invoke(mServer, e);
    }

    private static void ClientDisconnected(object? sender, TcpClient e)
    {
        mOnClientDisconnected?.Invoke(mServer, e);
    }


}

