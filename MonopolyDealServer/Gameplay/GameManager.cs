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
    static Deck? sDeck = null;
    static ImGuiValues mValues = new();
    public static void Start()
    {
        Server.mOnDataRecieved += Server_OnDataRecieved;
        Server.GameStarted();

        CurrentState = GameState.InGame;
        sDeck = new(Configuration.mDecksToUse);
        sDeck.LoadCardsFromFile();
        mValues.targetPlayerNumber = 1;
    }
    public static void SendInitialCards()
    {
        if (sDeck is null)
            return;

        var cards = sDeck.RemoveMultipleCardsFromDeck(GameData.PICK_UP_AMOUNT_ON_TURN_START);
        var data = Serializer.SerializeListOfCards(cards);
        Server.BroadcastMessage(ServerSendMessages.CardsSent, data, TurnManager.CurrentPlayer.Number);
    }

    private static void Server_OnDataRecieved(ulong clientID, int playerNumber, ClientSendMessages message, byte[] data)
    {
        var status = PlayerManager.TryGetPlayer(clientID, out var player);
        if (status != ConnectionStatus.Connected) 
            return;

        if (sDeck is null)
            return;

        switch (message)
        {
            case ClientSendMessages.PlaySlyDeal:
                DealManager.SlyDealPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.PlayForcedDeal:
                DealManager.ForcedDealPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.PlayDealBreaker:
                DealManager.DealBreakerPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.PlayPlunderCard:
                DealManager.PlunderCardPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.DealAccepted:
                DealManager.DealComplete();
                break;
            case ClientSendMessages.PlayBirthdayCard:
                PlayerActions.BirthdayPlayed(sDeck, player);
                break;
            case ClientSendMessages.PlayRentCard:
                PlayerActions.RentCardPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.PlayWildRentCard:
                PlayerActions.WildRentPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.PlayBuildingCard:
                PlayerActions.BuildingCardPlayed(player, data);
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
                PlayerActions.ActionCardPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.PlayDebtCollector:
                PlayerActions.DebtCollectorPlayed(sDeck, player, data);
                break;
            case ClientSendMessages.RejectedNo:
                var status1 = PlayerManager.TryGetPlayer(playerNumber, out var targetPlayer);
                if (status1 != ConnectionStatus.Connected)
                    break;

                if (PaymentManager.IsPaymentInProgress)
                    PaymentManager.NoRejected(sDeck, targetPlayer);

                if (DealManager.CurrentDealType != DealType.None)
                    DealManager.RecieverPlayedSayNo(sDeck, targetPlayer);

                break;
            case ClientSendMessages.ActionGotDenied:
                if (PaymentManager.IsPaymentInProgress)
                    PaymentManager.PlayerUsedSayNo(sDeck, player);

                if (DealManager.CurrentDealType != DealType.None)
                    DealManager.TargetPlayedSayNo(sDeck, player);
                break;
            case ClientSendMessages.RequestCards:
                PlayerActions.OnCardsRequested(sDeck, player, data);
                break;
            case ClientSendMessages.PutCardsBack:
                PlayerActions.PutCardsBack(sDeck, player, data);
                break;
            case ClientSendMessages.MoveCard:
                PlayerActions.MoveCard(player, data);
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
                var strs = Format.ToString(data).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (int.Parse(strs[0]) >= Configuration.mSetToPlayTo)
                {
                    Server.BroadcastMessage(ServerSendMessages.OnPlayerWin, player.Number);
                    End();
                    break;
                }    

                TurnManager.StartTurn(sDeck, player.Number, int.Parse(strs[1]));
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
        if (!ConnectionHandler.mUseDebugConsole)
            return;

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