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
    public static int ConnectedPlayers => mConnectedPlayers.Count;
    public static int DisconnectedPlayers => mDisconnectedPlayers.Count;
    public static bool DisconnectedPLayers => mDisconnectedPlayers.Count > 0;

    static PlayerManager()
    {
        mConnectedPlayers = new List<Player>();
        mDisconnectedPlayers = new List<Player>();
    }

    public static Player AddPlayer(TcpClient client, string name = "")
    {
        if (string.IsNullOrEmpty(name))
            name = $"Player {TotalPlayers + 1}";

        Player player = new(client, name, TotalPlayers);

        lock (mConnectedPlayers) 
        {
            mConnectedPlayers.Add(player);
        }
        
        return player;
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