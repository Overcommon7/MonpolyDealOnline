﻿using System.Text;
using SimpleTCP;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class PlayerActions
{
    public static void OnHandRequested(Deck deck, Player player, byte[] data, Message extra)
    {
        var cards = DataMarshal.GetCards(deck, Constants.PICK_UP_AMOUNT_ON_HAND_EMPTY);
        player.AddCardsToHand(cards);
        var message = Format.ToData(ServerSendMessages.HandReturned, Serializer.SerializeListOfCards(cards), player.Number);
        extra.Reply(message);
    }

    public static void CardPlayed(Deck deck, Player player, int cardID)
    {
        if (!CardData.TryGetCard(cardID, out Card card))
            return;

        if (player.RemoveCardFromHand(card))
            deck.AddCardToRemainingPile(card);
    }

    public static void PutCardsBack(Deck deck, Player player, byte[] data)
    {
        var cards = Serializer.GetCardsFromString<Card>(Format.ToString(data));
        foreach (var card in cards)
        {
            player.RemoveCardFromHand(card);
            deck.AddCardToRemainingPile(card);
        }

        Server.SendMessageExcluding(ServerSendMessages.UpdateCardsInHand, player.Number, Format.Encode(player.CardsInHand.ToString()), player.Number);       
    }

    public static void WildCardPlayed(Player player, byte[] data)
    {
        var wildCard = Format.ToStruct<PlayWildCard>(data);
        CardPlayed<WildCard>(player, wildCard.cardID, data, ServerSendMessages.PropertyCardPlayed, card => {
            card.SetCurrentType(wildCard.setType);
        });       
    }

    public static void RentCardPlayed(Player player, byte[] data)
    {
        //PaymentManager.StartNewPayment(player, )
    }

    public static void PropertyCardPlayed(Player player, byte[] data)
    {
        int cardID = int.Parse(Format.ToString(data));
        CardPlayed<MoneyCard>(player, cardID, data, ServerSendMessages.PropertyCardPlayed);
    }

    public static void MoneyCardPlayed(Player player, byte[] data)
    {
        int cardID = int.Parse(Format.ToString(data));
        CardPlayed<MoneyCard>(player, cardID, data, ServerSendMessages.MoneyCardPlayed);
    }

    static void CardPlayed<T>(Player player, int cardID, byte[] data, ServerSendMessages message, Action<T>? action = null) where T : Card
    {
        var card = CardData.CreateNewCard<T>(cardID);

        if (card is null)
            return;

        action?.Invoke(card);

        player.RemoveCardFromHand(card);
        player.AddCardToHand(card);       
        Server.SendMessageExcluding(message, player.Number, data, player.Number);
    }
}