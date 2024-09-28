﻿using System;
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

        public static void DebtCollectorPlayed(Gameplay gameplay, int playerNumber, byte[] data)
        {
            var localPlayerNumber = gameplay.PlayerManager.LocalPlayer.Number;
            if (playerNumber == localPlayerNumber)
                return;

            PaymentHandler.BeginPaymentProcess(playerNumber, Constants.DEBT_COLLECTOR_AMOUNT);
            var player = gameplay.PlayerManager.GetPlayer(playerNumber);

            var values = Format.ToStruct<DebtCollectorValues>(data);

            if (values.targetPlayerNumber == localPlayerNumber)
            {
                gameplay.GetWindow<PayPopup>().Open(gameplay.PlayerManager.LocalPlayer,
                    [$"{player.Name} Has Played Debt Collector",
                    $"You Owe M{Constants.DEBT_COLLECTOR_AMOUNT}"]);
            }                
            else
            {
                var targetPlayer = gameplay.PlayerManager.GetPlayer(values.targetPlayerNumber);
                gameplay.GetWindow<MessagePopup>().Open(
                    [$"{player.Name} Has Played Debt Collector On {targetPlayer.Name}",
                    $"{targetPlayer.Name} Owes M{Constants.DEBT_COLLECTOR_AMOUNT} To {player.Name}"]);
            }
              
        }
    }
}
