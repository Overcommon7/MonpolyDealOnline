using System;
using System.Collections.Generic;
namespace MonopolyDeal
{
    public class PlayerManager
    {
        int mCurrentPlayerNumbersTurn;
        List<OnlinePlayer> mPlayers;
        public LocalPlayer LocalPlayer { get; private set; }
        public IReadOnlyList<OnlinePlayer> OnlinePlayes => mPlayers;
        public Player GetPlayer(int playerNumber)
        {
            if (LocalPlayer.Number == playerNumber)
                return LocalPlayer;

            return GetOnlinePlayer(playerNumber);
        }
        public OnlinePlayer GetOnlinePlayer(int playerNumber)
        {
            int index = mPlayers.FindIndex(player => player.Number == playerNumber);
            if (index == -1)
                throw new System.InvalidOperationException();

            return mPlayers[index];
        }
        public Player CurrentTurnPlayer
        {
            get
            {
                if (LocalPlayer.Number == mCurrentPlayerNumbersTurn)
                    return LocalPlayer;

                int index = mPlayers.FindIndex(player => player.Number == mCurrentPlayerNumbersTurn);
                if (index == -1)
                     throw new System.InvalidOperationException();

                return mPlayers[index];                
            }
        }

        public PlayerManager()
        {
            var connection = App.GetState<Connection>();
            LocalPlayer = new LocalPlayer(connection.PlayerNumber, Client.ID, connection.Username);

            mPlayers = new();
            foreach (var onlinePlayer in connection.OtherPlayers)
                mPlayers.Add(new(onlinePlayer.Key, onlinePlayer.Value.Item2, onlinePlayer.Value.Item1));

            Client.SendData(ClientSendMessages.RequestHand, LocalPlayer.Number);
        }
        public void StartGame(int playerNumbersTurn)
        {

            mCurrentPlayerNumbersTurn = playerNumbersTurn;
        }
    }
}

