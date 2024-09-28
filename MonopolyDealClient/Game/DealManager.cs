namespace MonopolyDeal
{
    public static class DealManager
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
        public static void ActionPlayed(PlayerManager playerManager, ServerSendMessages message, int playerNumber, byte[] data)
        {
            CurrentDealType = GetDealType(message);
            PlayerAccepted = false;
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
                messageWindow.Open([dealMessage]);
            }
        }

        public static void TargetSaidNo()
        {

        }

        public static void RecieverRejectedNo()
        {

        }

        public static void TargetAccepted()
        {
            PlayerAccepted = true;               
        }

        public static void DealComplete(Gameplay gameplay)
        {
            CurrentDealType = DealType.None;
            mDealValues = new();


        }
    }
}