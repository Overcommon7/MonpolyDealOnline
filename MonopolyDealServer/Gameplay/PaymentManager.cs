using System;
using System.Collections.Generic;

public static class PaymentManager
{
    static TargetType sTargetType;
    static int sPlayersPaid;
    static Player? sPlayerBeingPaid;
    static List<int> sPlayersWhoSaidNo = new();
    public static bool IsPaymentInProgress { get; private set; } = false;
    public static void StartNewPayment(Player playerBeingPayed, TargetType targetType)
    {
        sTargetType = targetType;
        sPlayersPaid = 0;
        sPlayerBeingPaid = playerBeingPayed;
        IsPaymentInProgress = true;
        sPlayersWhoSaidNo.Clear();
    }

    public static void EndPayment()
    {
        sPlayerBeingPaid = null;
        sTargetType = TargetType.None;
        sPlayersPaid = 0;
        IsPaymentInProgress = false;
        sPlayersWhoSaidNo.Clear();
    }
    public static void PlayerUsedSayNo(Deck deck, Player player)
    {
        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DealBreaker, out var dealBreaker))
            return;

        if (!player.RemoveCardFromHand(dealBreaker))
            return;

        deck.AddCardToRemainingPile(dealBreaker);

        sPlayersWhoSaidNo.Add(player.Number);

        CheckForAllPlayersPaid();
    }
    public static void PlayerPaidCards(Player player, byte[] data)
    {
        if (sPlayerBeingPaid is null)
            return;

        string[] cards = Format.ToString(data).Split('#', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var notMoneyCards = Serializer.GetCardsFromString<Card>(cards[0]);
        var asMoneyCards = Serializer.GetCardsFromString<Card>(cards[1]);

        foreach (var card in notMoneyCards)
        {
            player.RemoveCardFromPlayArea(card);
            sPlayerBeingPaid.AddCardToPlayArea(card);
        }

        foreach (var card in asMoneyCards)
        {
            player.RemoveCardFromPlayArea(card);
            sPlayerBeingPaid.AddCardToPlayArea(card);
        }

        Server.SendMessageExcluding(ServerSendMessages.PlayerPaid, player.Number, data, player.Number);    
        CheckForAllPlayersPaid();
    }

    static void CheckForAllPlayersPaid()
    {
        if (sPlayerBeingPaid is null)
            return;

        ++sPlayersPaid;
        if (sTargetType == TargetType.One || (sTargetType == TargetType.All && sPlayersPaid >= PlayerManager.ConnectedPlayerCount))
        {
            Thread.Sleep(250);
            if (sPlayersWhoSaidNo.Count == 0)
            {
                Server.BroadcastMessage(ServerSendMessages.OnAllPlayersPaid, sPlayerBeingPaid.Number);
            }
            else
            {
                string playerIDs = string.Join(',', sPlayersWhoSaidNo);
                Server.BroadcastMessage(ServerSendMessages.PlayersUsedSayNo, playerIDs, sPlayerBeingPaid.Number);
            }
        }
    }
}

