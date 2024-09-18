using System.Text;
using SimpleTCP;

public static class PlayerActions
{
    public static void OnHandRequested(Deck deck, Player player, byte[] data, Message extra)
    {
        var cardData = DataMarshal.GetHand(deck);

    }
}