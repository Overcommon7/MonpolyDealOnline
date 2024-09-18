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
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < amount; i++)
        {
            stringBuilder.Append(deck.RemoveCardFromDeck().ID);
            if (i + 1 < amount)
                stringBuilder.Append(',');
        }

        return Format.Encode(stringBuilder.ToString());
    }
}

