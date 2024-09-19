using System.Text;
using SimpleTCP;

public static class PlayerActions
{
    public static void OnHandRequested(Deck deck, Player player, byte[] data, Message extra)
    {
        var cardData = DataMarshal.GetHand(deck);
        var message = Format.ToData(ServerSendMessages.HandReturned, cardData, player.Number);
        extra.Reply(message);
    }

    public static void CardPlayed(Deck deck, Player player, int cardID)
    {
        if (!CardData.TryGetCard(cardID, out Card card))
            return;

        if (player.RemoveCardFromHand(card))
            deck.AddCardToRemainingPile(card);
    }
}