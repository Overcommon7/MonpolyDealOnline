using System;
using System.Collections.Generic;

public static class DealManager
{
    public static DealType CurrentDealType = DealType.None;
    public static Player? RecievingPlayer = null;

    public static void SlyDealPlayed(Deck deck, Player player, byte[] data)
    {
        var values = Format.ToStruct<SlyDealValues>(data);

        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.SlyDeal, out var slyDeal))
            return;

        CurrentDealType = DealType.SlyDeal;
        PlayerActions.CardPlayedToDeck<ActionCard>(deck, player, slyDeal.ID, data, ServerSendMessages.SlyDealPlayed);
    }

    public static void ForcedDealPlayed(Deck deck, Player player, byte[] data)
    {
        var values = Format.ToStruct<DealBreakerValues>(data);

        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.ForcedDeal, out var forcedDeal))
            return;

        CurrentDealType = DealType.ForcedDeal;
        PlayerActions.CardPlayedToDeck<ActionCard>(deck, player, forcedDeal.ID, data, ServerSendMessages.ForcedDealPlayed);
    }

    public static void DealBreakerPlayed(Deck deck, Player player, byte[] data)
    {
        var values = Format.ToStruct<DealBreakerValues>(data);

        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DealBreaker, out var dealBreaker))
            return;

        CurrentDealType = DealType.DealBreaker;
        PlayerActions.CardPlayedToDeck<ActionCard>(deck, player, dealBreaker.ID, data, ServerSendMessages.DealBreakerPlayed);
    }
    public static void RecieverPlayedSayNo(Deck deck, Player player)
    {
        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var justSayNo))
            return;

        player.RemoveCardFromHand(justSayNo);
        deck.AddCardToRemainingPile(justSayNo);

        Server.BroadcastMessage(ServerSendMessages.NoWasRejected, player.Number);
    }
    public static void TargetPlayedSayNo(Deck deck, Player player)
    {
        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.JustSayNo, out var justSayNo))
            return;

        player.RemoveCardFromHand(justSayNo);
        deck.AddCardToRemainingPile(justSayNo);

        Server.BroadcastMessage(ServerSendMessages.JustSayNoPlayed, player.Number);
    }

    public static void DealComplete()
    {
        CurrentDealType = DealType.None;
        RecievingPlayer = null;

        Server.BroadcastMessage(ServerSendMessages.DealComplete, Constants.ALL_PLAYER_NUMBER);
    }
}

