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
        mValues.targetPlayerNumber = 1;
    }

    private static void Server_OnDataRecieved(ulong clientID, int playerNumber, ClientSendMessages message, byte[] data)
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
            case ClientSendMessages.PlayDealBreaker:
                break;
            case ClientSendMessages.PlayRentCard:
                PlayerActions.RentCardPlayed(player, data);
                break;
            case ClientSendMessages.PlayWildRentCard:
                break;
            case ClientSendMessages.PlayBuildingRentCard:
                break;
            case ClientSendMessages.PlayWildCard:
                PlayerActions.WildCardPlayed(player, data);
                break;
            case ClientSendMessages.PlayMoneyCard:
                PlayerActions.MoneyCardPlayed(player, data);
                break;
            case ClientSendMessages.PlayPropertyCard:
                PlayerActions.PropertyCardPlayed(player, data);
                break;
            case ClientSendMessages.PlayActionCard:
                PlayerActions.ActionCardPlayed(player, data);
                break;
            case ClientSendMessages.RejectedNo:
                if (PaymentManager.IsPaymentInProgress)
                {
                    var status1 = PlayerManager.TryGetPlayer(playerNumber, out var targetPlayer);
                    if (status1 != ConnectionStatus.Connected) 
                        break;

                    PaymentManager.NoRejected(sDeck, targetPlayer);
                }
                break;
            case ClientSendMessages.ActionGotDenied:
                if (PaymentManager.IsPaymentInProgress)
                    PaymentManager.PlayerUsedSayNo(sDeck, player);
                break;
            case ClientSendMessages.RequestCards:
                PlayerActions.OnCardsRequested(sDeck, player, data);
                break;
            case ClientSendMessages.PutCardsBack:
                PlayerActions.PutCardsBack(sDeck, player, data);
                break;
            case ClientSendMessages.MoveCard:
                break;
            case ClientSendMessages.PayPlayer:
                if (PaymentManager.IsPaymentInProgress)
                    PaymentManager.PlayerPaidCards(player, data);
                break;
            case ClientSendMessages.PaymentAccepted:
                if (PaymentManager.IsPaymentInProgress)
                {
                    PaymentManager.EndPayment();                    
                    Server.BroadcastMessage(ServerSendMessages.PaymentComplete, player.Number);
                }
                    

                
                break;
            case ClientSendMessages.OnEndTurn:
                TurnManager.EndTurn(sDeck);
                TurnManager.StartTurn(sDeck);
                break;
            case ClientSendMessages.ReadyForNextTurn:
                break;
            case ClientSendMessages.RequestHand:
                PlayerActions.OnHandRequested(sDeck, player, data);
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
        ImGui.Begin("Target Player", ImGuiWindowFlags.AlwaysAutoResize);
        {
            ImGui.InputInt("Target Player", ref mValues.targetPlayerNumber);
        }        
        ImGui.End();    

        ImGui.Begin("Give Cards");
        {
            int i = 0;
            foreach (var card in CardData.Cards)
            {
                if (!ImGui.Button($"Give {card.DisplayName()}##{i++}"))
                    continue;

                Server.BroadcastMessage(ServerSendMessages.DebugSendCard, Format.Encode(card.ID.ToString()), mValues.targetPlayerNumber);
            }
        }        
        ImGui.End();
    }
}