using System;
using System.Collections.Generic;

public static class PaymentManager
{
    static TargetType sTargetType;
    static int sPlayersPaid;
    static Player? sPlayerBeingPaid;
    static List<int> mPlayersWhoPaid = new List<int>();    
    public static bool IsPaymentInProgress { get; private set; } = false;
    
    public static void StartNewPayment(Player playerBeingPayed, TargetType targetType)
    {
        sTargetType = targetType;
        sPlayersPaid = 0;
        mPlayersWhoPaid.Clear();
        sPlayerBeingPaid = playerBeingPayed;
        IsPaymentInProgress = true;
    }

    public static void EndPayment()
    {
        sPlayerBeingPaid = null;
        sTargetType = TargetType.None;
        sPlayersPaid = 0;
        mPlayersWhoPaid.Clear();
        IsPaymentInProgress = false;
    }

    public static void NoRejected(Deck deck, Player targetPlayer)
    {
        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var justSayNo))
            return;

        sPlayerBeingPaid?.RemoveCardFromHand(justSayNo);
        mPlayersWhoPaid.Remove(targetPlayer.Number);

        deck.AddCardToRemainingPile(justSayNo);
        --sPlayersPaid;
       
        Server.BroadcastMessage(ServerSendMessages.NoWasRejected, targetPlayer.Number);
    }
    public static void PlayerUsedSayNo(Deck deck, Player player)
    {
        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var justSayNo))
            return;

        player.RemoveCardFromHand(justSayNo);          
        deck.AddCardToRemainingPile(justSayNo);

        Server.BroadcastMessage(ServerSendMessages.JustSayNoPlayed, player.Number);
        CheckForAllPlayersPaid();
    }
    public static void PlayerPaidCards(Player player, byte[] data)
    {
        if (sPlayerBeingPaid is null)
            return;

        if (mPlayersWhoPaid.Contains(player.Number))
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

        Server.BroadcastMessage(ServerSendMessages.PlayerPaid, data, player.Number);
        mPlayersWhoPaid.Add(player.Number);
        CheckForAllPlayersPaid();
    }

    static void CheckForAllPlayersPaid()
    {
        if (sPlayerBeingPaid is null)
            return;

        ++sPlayersPaid;
        if (sTargetType == TargetType.One || (sTargetType == TargetType.All && sPlayersPaid >= PlayerManager.ConnectedPlayerCount - 1))
        {
            Server.BroadcastMessage(ServerSendMessages.OnAllPlayersPaid, sPlayerBeingPaid.Number);
        }
    }
}

