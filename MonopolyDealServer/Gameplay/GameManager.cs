using ImGuiNET;
using SimpleTCP;

public enum GameState
{
    Lobby,
    InGame
}
public static class GameManager
{
    struct ImGuiValues
    {
        public int targetPlayerNumber;
    }
    public static GameState CurrentState { get; set; } = GameState.Lobby;
    public static Configuration Configuration { get; set; } = new();
    static Deck sDeck;
    static ImGuiValues mValues = new();
    public static void Start()
    {
        Server.mOnDataRecieved += Server_OnDataRecieved;
        CurrentState = GameState.InGame;
        sDeck = new(Configuration.mDecksToUse);
        sDeck.LoadCardsFromFile();
    }

    private static void Server_OnDataRecieved(ulong clientID, ClientSendMessages message, byte[] data, Message extra)
    {
        var status = PlayerManager.TryGetPlayer(clientID, out var player);
        if (status != ConnectionStatus.Connected) 
            return;

        switch (message)
        {
            case ClientSendMessages.PlaySlyDeal:
                
                break;
            case ClientSendMessages.PlayForcedDeal:
                break;
            case ClientSendMessages.PlayRentCard:
                PlayerActions.RentCardPlayed(player, data);
                break;
            case ClientSendMessages.PlayWildCard:
                PlayerActions.WildCardPlayed(player, data);
                break;
            case ClientSendMessages.RequestCards:
                break;
            case ClientSendMessages.PutCardsBack:
                PlayerActions.PutCardsBack(sDeck, player, data);
                break;
            case ClientSendMessages.MoveCard:
                break;
            case ClientSendMessages.PayPlayer:

                break;
            case ClientSendMessages.OnEndTurn:
                TurnManager.EndTurn(sDeck);
                TurnManager.StartTurn(sDeck);
                break;
            case ClientSendMessages.ReadyForNextTurn:
                break;
            case ClientSendMessages.RequestHand:
                PlayerActions.OnHandRequested(sDeck, player, data, extra);
                break;
        }
    }

    public static void End()
    {
        Server.mOnDataRecieved -= Server_OnDataRecieved;
        CurrentState = GameState.Lobby;
        sDeck = new(Configuration.mDecksToUse);
    }

    public static void ImGuiDraw()
    {
        ImGui.Begin("Debug Window");

        ImGui.InputInt("Target Player", ref mValues.targetPlayerNumber);

        if (ImGui.TreeNode("Give Card To Player"))
        {
            foreach (var card in CardData.Cards)
            {
                if (card is RentCard rent)
                {
                    if (!ImGui.Button($"Give {card.Name} - {rent.TargetType1}-{rent.TargetType2}##{card.ID}"))
                        continue;
                }
                else
                {
                    if (!ImGui.Button($"Give {card.Name}##{card.ID}"))
                        continue;
                }

                
                Server.SendMessageToPlayers(ServerSendMessages.DebugSendCard, 0, Format.Encode(card.ID.ToString()), mValues.targetPlayerNumber);
            }

            ImGui.TreePop();
        }

        ImGui.End();
    }
}