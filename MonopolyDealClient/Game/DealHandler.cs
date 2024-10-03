using SimpleTCP;
using System;
using Windows.Gaming.Input;
using Windows.Media.PlayTo;

namespace MonopolyDeal
{
    public static class DealHandler
    {
        struct DealValues
        {
            public Player recievingPlayer;
            public Player targetPlayer;
            public object data;
        }
        public static DealType CurrentDealType { get; private set; }
        public static bool IsDealInProgress  => CurrentDealType != DealType.None;
        public static bool PlayerAccepted { get; private set; } = false;

        static bool mTargetSaidNo = false;
        static DealValues mDealValues = new();

        static DealType GetDealType(ServerSendMessages message)
        {
            if (message == ServerSendMessages.DealBreakerPlayed)
                return DealType.DealBreaker;
            if (message == ServerSendMessages.SlyDealPlayed)
                return DealType.SlyDeal;
            if (message == ServerSendMessages.ForcedDealPlayed)
                return DealType.ForcedDeal;
            if (message == ServerSendMessages.PlunderDealPlayed)
                return DealType.Plunder;

            return DealType.None;
        }
        public static void StartDeal(PlayerManager playerManager, ServerSendMessages message, int playerNumber, byte[] data)
        {
            CurrentDealType = GetDealType(message);
            PlayerAccepted = false;
            mTargetSaidNo = false;
            mDealValues.recievingPlayer = playerManager.GetPlayer(playerNumber);

            string dealMessage = string.Empty;

            switch (CurrentDealType)
            {
                case DealType.SlyDeal:
                {
                    var values = Format.ToStruct<SlyDealValues>(data);
                    mDealValues.targetPlayer = playerManager.GetPlayer(values.targetPlayerNumber);
                    mDealValues.data = values;
                    if (CardData.TryGetCard<Card>(values.cardID, out var card))
                        dealMessage = $"{mDealValues.recievingPlayer.Name} Has Sly Dealed {mDealValues.targetPlayer.Name}'s {card.DisplayName()}.";
                }
                break;
                case DealType.ForcedDeal:
                {
                    var values = Format.ToStruct<ForcedDealValues>(data);
                    mDealValues.targetPlayer = playerManager.GetPlayer(values.playerTradingWithNumber);
                    mDealValues.data = values;
                    if (CardData.TryGetCard<Card>(values.givingToPlayerCardID, out var giving) && 
                        CardData.TryGetCard<Card>(values.takingFromPlayerCardID, out var taking))
                    {
                        dealMessage = $"{mDealValues.recievingPlayer.Name} Has Forced Dealed Their {giving.DisplayName()} Card For {mDealValues.targetPlayer.Name}'s {taking.DisplayName()} Property.";
                    }
                }
                break;
                case DealType.DealBreaker:
                {
                    var values = Format.ToStruct<DealBreakerValues>(data);
                    mDealValues.targetPlayer = playerManager.GetPlayer(values.targetPlayerNumber);
                    mDealValues.data = values;
                    dealMessage = $"{mDealValues.recievingPlayer.Name} Has Deal Broken {mDealValues.targetPlayer.Name} For Their {values.setType} Properties.";
                }
                break;
                case DealType.Plunder:
                {
                    var values = Format.ToStruct<PlunderDealValues>(data);
                    mDealValues.targetPlayer = playerManager.GetPlayer(values.targetPlayerNumber);
                    mDealValues.data = values;
                    dealMessage = $"{mDealValues.recievingPlayer.Name} Has Plundered ";
                    if (mDealValues.targetPlayer.Number == playerManager.LocalPlayer.Number)
                        dealMessage += $"Your {CardData.CreateNewCard<Card>(values.cardID)?.DisplayName()}";
                    else
                        dealMessage += mDealValues.targetPlayer.Name;
                }
                break;
            }  
            
            if (playerManager.LocalPlayer.Number == mDealValues.targetPlayer.Number)
            {
                var dealWindow = App.GetState<Gameplay>().GetWindow<GettingDealWindow>();
                dealWindow.Open(mDealValues.recievingPlayer, mDealValues.targetPlayer, dealMessage, playerManager.LocalPlayer.Number);
            }
            else if (playerManager.LocalPlayer.Number != mDealValues.recievingPlayer.Number)
            {
                var messageWindow = App.GetState<Gameplay>().GetWindow<MessagePopup>();
                messageWindow.Open([dealMessage], false);
            }
        }

        public static void SaidNoPlayed(PlayerManager playerManager, int playerNumberWhoPlayedNo)
        {
            if (mDealValues.targetPlayer.Number == playerNumberWhoPlayedNo)
                TargetSaidNo(playerManager);

            if (mDealValues.recievingPlayer.Number == playerNumberWhoPlayedNo)
                RecieverRejectedNo(playerManager);
        }

        static void TargetSaidNo(PlayerManager playerManager)
        {
            var localPlayer = playerManager.LocalPlayer;
            mTargetSaidNo = true;

            if (localPlayer.Number == mDealValues.recievingPlayer.Number)
            {
                var dealWindow = App.GetState<Gameplay>().GetWindow<GettingDealWindow>();
                string message =  $"{mDealValues.recievingPlayer.Name} Rejected Your No";
                if (!dealWindow.IsOpen)
                    dealWindow.Open(mDealValues.recievingPlayer, mDealValues.targetPlayer, message, localPlayer.Number);
                else
                    dealWindow.ChangeMessage(message);

                dealWindow.GotRejected();
            }
            else if (localPlayer.Number != mDealValues.targetPlayer.Number)
            {
                var messageWindow = App.GetState<Gameplay>().GetWindow<MessagePopup>();
                string message = $"{mDealValues.targetPlayer.Name} Played \"Just Say No\"";
                if (!messageWindow.IsOpen)
                    messageWindow.Open([message]);
                else
                    messageWindow.AddMessage(message);
            }
        }

        static void RecieverRejectedNo(PlayerManager playerManager)
        {
            var localPlayer = playerManager.LocalPlayer;
            mTargetSaidNo = false;

            if (localPlayer.Number == mDealValues.targetPlayer.Number)
            {
                var dealWindow = App.GetState<Gameplay>().GetWindow<GettingDealWindow>();
                dealWindow.GotRejected();
                return;
            }
            else if (localPlayer.Number != mDealValues.recievingPlayer.Number)
            {
                var messageWindow = App.GetState<Gameplay>().GetWindow<MessagePopup>();
                string message = $"{mDealValues.recievingPlayer.Name} Played Rejected {mDealValues.targetPlayer.Name}'s \"Just Say No\"";
                if (!messageWindow.IsOpen)
                    messageWindow.Open([message]);
                else
                    messageWindow.AddMessage(message);
            }

            if (mDealValues.targetPlayer is OnlinePlayer onlinePlayer)
                --onlinePlayer.CardsInHand;
        }

        public static void TargetAccepted()
        {
            PlayerAccepted = true;               
        }

        static void DoDealLogic(PlayerManager playerManager)
        {
            if (mTargetSaidNo)
                return;

            switch (CurrentDealType)
            {
                case DealType.SlyDeal:
                    DoSlyDealLogic(playerManager, (SlyDealValues)mDealValues.data);
                    break;
                case DealType.ForcedDeal:
                    DoForcedDealLogic(playerManager, (ForcedDealValues)mDealValues.data);
                    break;
                case DealType.DealBreaker:
                    DoDealBreakerLogic(playerManager, (DealBreakerValues)mDealValues.data);
                    break;
                case DealType.Plunder:
                    DoPlunderLogic(playerManager, (PlunderDealValues)mDealValues.data);
                    break;
            }
        }

        private static void DoPlunderLogic(PlayerManager playerManager, PlunderDealValues values)
        {
            {
                if (mDealValues.targetPlayer is OnlinePlayer online)
                {
                    --online.CardsInHand;                    
                }
                else if (mDealValues.targetPlayer is LocalPlayer local)
                {
                    if (!CardData.TryGetCard<Card>(values.cardID, out var newCard))
                        return;

                    if (newCard is not null)
                        local.Hand.RemoveCard(newCard);
                }
            }
            {
                if (mDealValues.recievingPlayer is OnlinePlayer online)
                {
                   ++online.CardsInHand;
                }
                else if (mDealValues.recievingPlayer is LocalPlayer local)
                {
                    var newCard = CardData.CreateNewCard<Card>(values.cardID);

                    if (newCard is not null)
                        local.Hand.AddCard(newCard);

                    var dealWindow = App.GetState<Gameplay>().GetWindow<GettingDealWindow>();
                    if (dealWindow.IsOpen)
                        dealWindow.ChangeMessage($"You Plundered {mDealValues.targetPlayer}'s {newCard.DisplayName()}");
                }


            }
        }

        static void DoDealBreakerLogic(PlayerManager playerManager, DealBreakerValues values)
        {
            var cards = mDealValues.targetPlayer.PlayedCards.GetPropertyCardsOfType(values.setType);
            var amountForSet = CardData.GetValues(values.setType).AmountForFullSet;

            if (cards.Length > amountForSet)
                Array.Sort(cards, CardData.SortAlgorithm);

            for (int i = 0; i < amountForSet; ++i)
            {
                mDealValues.targetPlayer.PlayedCards.RemovePropertyCard(cards[i]);
                if (cards[i] is WildCard wild)
                    wild.SetCurrentType(values.setType);
                mDealValues.recievingPlayer.PlayedCards.AddPropertyCard(cards[i]);
            }

            var house = mDealValues.targetPlayer.PlayedCards.GetBuildingCard(ActionType.House, values.setType);
            if (house is not null)
            {
                mDealValues.targetPlayer.PlayedCards.RemoveBuildingCard(house);
                mDealValues.recievingPlayer.PlayedCards.AddBuildingCard(house, values.setType);
            }

            var hotel = mDealValues.targetPlayer.PlayedCards.GetBuildingCard(ActionType.Hotel, values.setType);
            if (hotel is not null)
            {
                mDealValues.targetPlayer.PlayedCards.RemoveBuildingCard(hotel);
                mDealValues.recievingPlayer.PlayedCards.AddBuildingCard(hotel, values.setType);
            }

        }

        static void DoSlyDealLogic(PlayerManager playerManager, SlyDealValues values)
        {
            var card = CardData.CreateNewCard<PropertyCard>(values.cardID);
            if (card is null)
                return;
            
            if (card is WildCard wild)
            {
                wild.SetCurrentType(values.setType);
            }

            mDealValues.targetPlayer.PlayedCards.RemovePropertyCard(card);
            mDealValues.recievingPlayer.PlayedCards.AddPropertyCard(card);
        }

        static void DoForcedDealLogic(PlayerManager playerManager, ForcedDealValues values)
        {
            var takingCard = CardData.CreateNewCard<PropertyCard>(values.takingFromPlayerCardID);
            var givingCard = CardData.CreateNewCard<PropertyCard>(values.givingToPlayerCardID);

            if (takingCard is null) return;
            if (givingCard is null) return;

            {
                if (takingCard is WildCard wild)
                    wild.SetCurrentType(values.takingSetType);
            }
            {
                if (givingCard is WildCard wild)
                    wild.SetCurrentType(values.givingSetType);
            }            

            mDealValues.targetPlayer.PlayedCards.RemovePropertyCard(takingCard);
            mDealValues.recievingPlayer.PlayedCards.AddPropertyCard(takingCard);

            mDealValues.targetPlayer.PlayedCards.AddPropertyCard(givingCard);
            mDealValues.recievingPlayer.PlayedCards.RemovePropertyCard(givingCard);
        }

        public static void DealComplete(Gameplay gameplay)
        {
            DoDealLogic(gameplay.PlayerManager);

            var player = gameplay.PlayerManager.LocalPlayer;
            if (player.Number == mDealValues.targetPlayer.Number || player.Number == mDealValues.recievingPlayer.Number)
            {
                gameplay.GetWindow<GettingDealWindow>().ShowAcceptButton = true;
            }
            else
            {
                gameplay.GetWindow<MessagePopup>().ShowCloseButton = true;
            }

            CurrentDealType = DealType.None;
            mDealValues = new();
        }
    }
}