using System.Net.Sockets;

public enum ConnectionStatus
{
    Invalid,
    Connected,
    Disconnected,   
}

public static class PlayerManager
{
    static List<Player> mConnectedPlayers;
    static List<Player> mDisconnectedPlayers;

    public static int TotalPlayers => mConnectedPlayers.Count + mDisconnectedPlayers.Count;
    public static int ConnectedPlayerCount => mConnectedPlayers.Count;
    public static int DisconnectedPlayerCount => mDisconnectedPlayers.Count;
    public static bool DisconnectedPLayers => mDisconnectedPlayers.Count > 0;
    public static IReadOnlyList<Player> ConnectedPlayers => mConnectedPlayers;
    static PlayerManager()
    {
        mConnectedPlayers = new List<Player>();
        mDisconnectedPlayers = new List<Player>();
    }

    public static Player AddPlayer(TcpClient client, string name = "")
    {
        lock (mConnectedPlayers)
        {
            if (string.IsNullOrEmpty(name))
            name = $"Player {TotalPlayers + 1}";

            Player player = new(client, name, TotalPlayers + 1);

            mConnectedPlayers.Add(player);
            return player;
        }        
    }

    public static ConnectionStatus TryGetPlayer(TcpClient client, out Player player)
    {
        player = null;

        if (!client.Connected)
            return ConnectionStatus.Invalid;

        ulong id = client.GetID();
        if (id == 0) return ConnectionStatus.Invalid;

        return GetStatus(out player, p => p.ID == id);
    }

    public static ConnectionStatus TryGetPlayer(ulong playerID, out Player player)
    {
        return GetStatus(out player, p => p.ID == playerID);
    }

    public static void PlayerReconnected(Player player)
    {
        lock (mDisconnectedPlayers)
        {
            mDisconnectedPlayers.Remove(player);
        }

        lock (mConnectedPlayers)
        {
            if (!mConnectedPlayers.Contains(player))
            {
                mConnectedPlayers.Add(player);
                mConnectedPlayers.Sort((a, b) => a.Number.CompareTo(b.Number));
            }               
        }       
    }

    public static void PlayerDisconnected(Player player)
    {
        lock (mDisconnectedPlayers)
        {
            if (!mDisconnectedPlayers.Contains(player))
                mDisconnectedPlayers.Add(player);
        }

        lock (mConnectedPlayers)
        {
            if (mConnectedPlayers.Remove(player))
                mConnectedPlayers.Sort((a, b) => a.Number.CompareTo(b.Number));
        }
    }

    public static ConnectionStatus TryGetPlayer(int playerNumber, out Player player)
    {
        return GetStatus(out player, p => p.Number == playerNumber);
    }

    public static TcpClient[] GetClientsExcluding(int excludedPlayerNumber)
    {
        TcpClient[] clients = new TcpClient[mConnectedPlayers.Count - 1];
        for (int i = 0, j = 0; i < mConnectedPlayers.Count; ++i)
        {
            if (mConnectedPlayers[i].Number != excludedPlayerNumber)
                clients[j++] = mConnectedPlayers[i].Client;
        }

        return clients;
    }

    static ConnectionStatus GetStatus(out Player player, Predicate<Player> predicate) 
    {
        player = mConnectedPlayers.Find(predicate);
        if (player is not null)
            return ConnectionStatus.Connected;


        player = mDisconnectedPlayers.Find(predicate);
        if (player is not null)
            return ConnectionStatus.Disconnected;

        return ConnectionStatus.Invalid;
    }

    
}