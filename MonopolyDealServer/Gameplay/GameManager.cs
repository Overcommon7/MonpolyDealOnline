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
    static Deck sDeck;
    public static void Start()
    {
        Server.mOnDataRecieved += Server_OnDataRecieved;
        CurrentState = GameState.InGame;
        sDeck = new(Configuration.mDecksToUse);
    }

    private static void Server_OnDataRecieved(ulong clientID, ClientSendMessages message, byte[] data, Message extra)
    {
        var status = PlayerManager.TryGetPlayer(clientID, out var player);
        if (status != ConnectionStatus.Connected) 
            return;

        switch (message)
        {
            case ClientSendMessages.PlayStealCard:
                break;
            case ClientSendMessages.PlayForcedDeal:
                break;
            case ClientSendMessages.PlayRentCard:
                break;
            case ClientSendMessages.PlayWildCard:
                break;
            case ClientSendMessages.RemoveCardsFromHand:
                break;
            case ClientSendMessages.RemoveCardsFromPlayArea:
                break;
            case ClientSendMessages.RequestCards:
                break;
            case ClientSendMessages.MoveCard:
                break;
            case ClientSendMessages.PayPlayer:
                break;
            case ClientSendMessages.OnTurnEnded:
                break;
            case ClientSendMessages.ReadyForNextTurn:
                break;
            case ClientSendMessages.RequestHand:
                PlayerActions.OnHandRequested(player, data, extra);
                break;
        }
    }

    public static void End()
    {

    }
}