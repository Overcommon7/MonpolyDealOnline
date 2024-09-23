using System.Text;
using SimpleTCP;

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
        var wildCardData = Format.ToStruct<PlayWildCard>(data);
        var wildCard = CardData.CreateNewCard<WildCard>(wildCardData.cardID);

        if (wildCard is null)
            return;

        wildCard.SetCurrentType(wildCard.SetType);
        player.RemoveCardFromHand(wildCard);
        player.AddCardToPlayArea(wildCard);

        Server.SendMessageExcluding(ServerSendMessages.WildCardPlayed, player.Number, data, player.Number);
    }

    public static void RentCardPlayed(Player player, byte[] data)
    {
        //PaymentManager.StartNewPayment(player, )
    }

    public static void PropertyCardPlayed(Player player, byte[] data)
    {
        int cardID = int.Parse(Format.ToString(data));
        var property = CardData.CreateNewCard<PropertyCard>(cardID);

        if (property is null) 
            return;

        player.RemoveCardFromHand(property);
        player.AddCardToHand(property);

        Server.SendMessageExcluding(ServerSendMessages.PropertyCardPlayed, player.Number, data, player.Number);
    }
}