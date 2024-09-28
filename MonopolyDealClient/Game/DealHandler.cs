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
                    if (CardData.TryGetCard<Card>(values.givingToPlayerID, out var giving) && 
                        CardData.TryGetCard<Card>(values.takingFromPlayerID, out var taking))
                    {
                        dealMessage = $"{mDealValues.recievingPlayer.Name} Has Forced Dealed Their {giving.DisplayName()} Card For {mDealValues.targetPlayer.Name}'s {taking.DisplayName()} Properties.";
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
                dealWindow.GotRejected();
            }
            else if (localPlayer.Number != mDealValues.targetPlayer.Number)
            {
                var messageWindow = App.GetState<Gameplay>().GetWindow<MessagePopup>();
                messageWindow.AddMessage($"{mDealValues.targetPlayer.Name} Played \"Just Say No\"");
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
                messageWindow.AddMessage($"{mDealValues.recievingPlayer.Name} Played Rejected {mDealValues.targetPlayer.Name}'s \"Just Say No\"");
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
            
        }

        static void DoForcedDealLogic(PlayerManager playerManager, ForcedDealValues values)
        {
            
        }

        public static void DealComplete(Gameplay gameplay)
        {
            CurrentDealType = DealType.None;
            mDealValues = new();

            var player = gameplay.PlayerManager.LocalPlayer;
            if (player.Number == mDealValues.targetPlayer.Number || player.Number == mDealValues.recievingPlayer.Number)
            {
                gameplay.GetWindow<GettingDealWindow>().ShowAcceptButton = true;
            }
            else
            {
                gameplay.GetWindow<MessagePopup>().ShowCloseButton = true;
            }
        }
    }
}