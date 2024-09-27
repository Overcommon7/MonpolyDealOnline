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
            mDealValues.recievingPlayer = playerManager.GetPlayer(playerNumber);

            switch (CurrentDealType)
            {
                case DealType.SlyDeal:
                {
                    var values = Format.ToStruct<SlyDealValues>(data);
                    mDealValues.targetPlayer = playerManager.GetPlayer(values.targetPlayerNumber);
                    mDealValues.data = values;
                }
                break;
                case DealType.ForcedDeal:
                {
                    var values = Format.ToStruct<ForcedDealValues>(data);
                    mDealValues.targetPlayer = playerManager.GetPlayer(values.playerTradingWithNumber);
                    mDealValues.data = values;
                }
                break;
                case DealType.DealBreaker:
                {
                    var values = Format.ToStruct<DealBreakerValues>(data);
                    mDealValues.targetPlayer = playerManager.GetPlayer(values.targetPlayerNumber);
                    mDealValues.data = values;
                }
                break;
            }            
        }

        public static void DealComplete()
        {
            CurrentDealType = DealType.None;
            mDealValues = new();
        }
    }
}