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

            var wildCardData = Format.ToStruct<PlayWildCard>(data);
            OnlinePlayerPlayedCard<WildCard>(playerManager, playerNumber, wildCardData.cardID, (player, card) =>
            {
                card.SetCurrentType(wildCardData.setType);
                player.PlayedCards.AddPropertyCard(card);
            });
        }

        public static void MoneyCardPlayed(PlayerManager playerManager, int playerNumber, byte[] data)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            int cardID = int.Parse(Format.ToString(data));
            OnlinePlayerPlayedCard<Card>(playerManager, playerNumber, cardID, (player, card) =>
            {
                if (card is ActionCard action)
                    action.SetAsMoney(true);

                player.PlayedCards.AddMoneyCard(card);
            });
        }

        public static void PropertyCardPlayed(PlayerManager playerManager, int playerNumber, byte[] data)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            int cardID = int.Parse(Format.ToString(data));
            OnlinePlayerPlayedCard<PropertyCard>(playerManager, playerNumber, cardID, (player, card) =>
            {
                player.PlayedCards.AddPropertyCard(card);
            });
        }

        public static void RentCardPlayed(PayPopup pay, PlayerManager playerManager, int playerNumber, byte[] data)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            var rentValues = Format.ToStruct<RentPlayValues>(data);
            if (!CardData.TryGetCard<RentCard>(rentValues.cardID, out var card))
                return;

            int rentAmount = CardData.GetRentAmount(rentValues.chargingSetType, rentValues.cardsOwnedInSet);
            var player = playerManager.GetOnlinePlayer(playerNumber);
            if (rentValues.withDoubleRent)
            {
                --player.CardsInHand;
                rentAmount *= 2;
            }
                

            App.GetState<Gameplay>().SetToRespondingState();
            PaymentHandler.BeginPaymentProcess(playerNumber, rentAmount);

            --player.CardsInHand;

            string[] messages =
                [
                    $"Player {player.Name} Has Charged Rent On Their {rentValues.chargingSetType} Properties",
                    $"You Owe M{rentAmount}"
                ];

            pay.Open(playerManager.LocalPlayer, messages);
        }

        static void OnlinePlayerPlayedCard<T>(PlayerManager playerManager, int playerNumber, int cardID, Action<OnlinePlayer, T> action) where T : Card
        {
            var player = playerManager.GetOnlinePlayer(playerNumber);
            var card = CardData.CreateNewCard<T>(cardID);

            if (card is null)
                return;

            --player.CardsInHand;

            action(player, card);
        }

        public static void DealBreakerPlayed(MessagePopup messagePopup, PlayerManager playerManager, int playerNumber, byte[] data)
        {
            var values = Format.ToStruct<DealBreakerValues>(data);

            var localPlayer = playerManager.LocalPlayer;
            var targetPlayer = playerManager.GetPlayer(values.targetPlayerNumber);
            var recievingPlayer = playerManager.GetPlayer(playerNumber);

            var cards = targetPlayer.PlayedCards.GetPropertyCardsOfType(values.setType);
            foreach (var card in cards)
            {
                targetPlayer.PlayedCards.RemovePropertyCard(card);
                recievingPlayer.PlayedCards.AddPropertyCard(card);
            }

            var house = targetPlayer.PlayedCards.GetBuildingCard(ActionType.House, values.setType);
            if (house is not null)
            {
                targetPlayer.PlayedCards.RemoveBuildingCard(house);
                recievingPlayer.PlayedCards.AddBuildingCard(house, values.setType);
            }

            var hotel = targetPlayer.PlayedCards.GetBuildingCard(ActionType.Hotel, values.setType);
            if (hotel is not null)
            {
                targetPlayer.PlayedCards.RemoveBuildingCard(hotel);
                recievingPlayer.PlayedCards.AddBuildingCard(hotel, values.setType);
            }

            if (localPlayer.Number != recievingPlayer.Number && localPlayer.Number != targetPlayer.Number)
            {
                messagePopup.Open(["Deal Breaker Played", $"{recievingPlayer.Name} Took The {values.setType} Properties From {targetPlayer.Name}"]);
            }

        }

        public static void OnActionCardPlayed(PlayerManager playerManager, int playerNumber, byte[] data)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            var player = playerManager.GetOnlinePlayer(playerNumber);
            var values = Format.ToStruct<PlayActionCardValues>(data);
            if (values.asMoney)
            {
                if (CardData.TryGetCard<ActionCard>(values.cardID, out var card))
                    player.PlayedCards.AddMoneyCard(card);
            }

            --player.CardsInHand;
        }
    }
}
