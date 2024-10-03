using System;
using System.Collections.Generic;

public static class DealManager
{
    public static DealType CurrentDealType = DealType.None;

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
        var values = Format.ToStruct<ForcedDealValues>(data);

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
        Server.BroadcastMessage(ServerSendMessages.DealComplete, GameData.ALL_PLAYER_NUMBER);
    }

    public static void PlunderCardPlayed(Deck deck, Player player, byte[] data)
    {
        CurrentDealType = DealType.Plunder;
        var values = Format.ToStruct<PlunderDealValues>(data);

        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.Plunder, out var plunder))
            return;

        var status = PlayerManager.TryGetPlayer(values.targetPlayerNumber, out var target);
        if (status != ConnectionStatus.Connected)
            return;



        var card = target.GetCardFromHand(values.handIndex);
        target.RemoveCardFromHand(card);

        player.RemoveCardFromHand(plunder);
        deck.AddCardToRemainingPile(plunder);

        player.AddCardToHand(card);
        values.cardID = card.ID;

        Server.BroadcastMessage(ServerSendMessages.PlunderDealPlayed, ref values, player.Number);
    }
}

