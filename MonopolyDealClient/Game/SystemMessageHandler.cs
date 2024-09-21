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

        public static void TurnStarted(PlayerManager playerManager, byte[] data)
        {
            string dataString = Format.ToString(data);
            foreach (var playerData in dataString.Split('|'))
            {
                var strs = playerData.Split(',');
                int number = int.Parse(strs[0]);

                if (number == playerManager.LocalPlayer.Number)
                    continue;

                var player = playerManager.GetOnlinePlayer(number);
                player.CardsInHand = int.Parse(strs[1]);
            }
        }

        public static void OnlinePlayerHandUpdate(PlayerManager playerManager, byte[] data, int playerNumber)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            var player = playerManager.GetOnlinePlayer(playerNumber);
            string numberOfCards = Format.ToString(data);
            player.CardsInHand = int.Parse(numberOfCards);
        }

        public static void WildCardPlayed(PlayerManager playerManager, int playerNumber, byte[] data)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            var player = playerManager.GetOnlinePlayer(playerNumber);
            var wildCardData = Format.ToStruct<PlayWildCard>(data);
            var wildCard = CardData.CreateNewCard<WildCard>(wildCardData.cardID);
            wildCard.SetCurrentType(wildCard.SetType);
            player.PlayedCards.AddPropertyCard(wildCard);
            --player.CardsInHand;
        }
    }
}
