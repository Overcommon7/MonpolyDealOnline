using SimpleTCP;

public enum GameState
{
    Lobby,
    InGame
}
public static class GameManager
{
    public static GameState CurrentState { get; set; } = GameState.Lobby;
    public static Configuration Configuration { get; set; } = new();
    public static void Start()
    {
        Server.mOnDataRecieved += Server_OnDataRecieved;

    }

    private static void Server_OnDataRecieved(ulong clientID, ClientSendMessages message, byte[] data, Message extra)
    {
        
    }

    public static void End()
    {

    }
}