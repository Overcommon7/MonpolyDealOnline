using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public static class SystemMessageHandler
    {
        public static void OnCardsRecieved(PlayerManager playerManager, byte[] data, int playerNumber)
        {
            var stringData = Format.ToString(data);

            if (playerNumber == playerManager.LocalPlayer.Number)
            {                
                var cards = Serializer.GetCardsFromString<Card>(stringData);
                playerManager.LocalPlayer.Hand.AddCards(cards);
            }
            else
            {
                var player = playerManager.GetOnlinePlayer(playerNumber);
                player.CardsInHand += Serializer.GetNumberOfCardsInString(stringData);
            }
        }
    }
}
