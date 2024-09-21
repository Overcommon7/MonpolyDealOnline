using System;
using System.Collections.Generic;
using System.Text;
public static class DataMarshal
{
    public static byte[] GetHand(Deck deck)
    {
        return GetCards(deck, Constants.PICK_UP_AMOUNT_ON_GAME_START);
    }

    public static byte[] GetCards(Deck deck, int amount)
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < amount; i++)
        {
            cards.Add(deck.RemoveCardFromDeck());
        }

        return Format.Encode(Serializer.SerializeListOfCards(cards));
    }
}

