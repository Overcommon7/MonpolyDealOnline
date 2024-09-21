using System;
using System.Collections.Generic;
using System.Text;
public static class DataMarshal
{

    public static List<Card> GetCards(Deck deck, int amount)
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < amount; i++)
        {
            cards.Add(deck.RemoveCardFromDeck());
        }

        return cards;
    }
}

