using System;
using System.Collections.Generic;
namespace MonopolyDeal
{
    public class PlayerManager
    {
        int mCurrentPlayerNumbersTurn;
        List<OnlinePlayer> mPlayers;
        public LocalPlayer Player { get; private set; }
        IReadOnlyList<OnlinePlayer> OnlinePlayes => mPlayers;
        public Player GetPlayer(int playerNumber)
        {
            if (Player.Number == playerNumber)
                return Player;

            return GetOnlinePlayer(playerNumber);
        }
        public OnlinePlayer GetOnlinePlayer(int playerNumber)
        {
            int index = mPlayers.FindIndex(player => player.Number == playerNumber);
            if (index == -1)
                throw new System.InvalidOperationException();

            return mPlayers[index];
        }
        public Player CurrentPlayer
        {
            get
            {
                if (Player.Number == mCurrentPlayerNumbersTurn)
                    return Player;

                int index = mPlayers.FindIndex(player => player.Number == mCurrentPlayerNumbersTurn);
                if (index == -1)
                     throw new System.InvalidOperationException();

                return mPlayers[index];                
            }
        }

        public PlayerManager() 
        {
            var connection = App.GetState<Connection>();
            Player = new LocalPlayer(connection.PlayerNumber, Client.ID, connection.Username);

            mPlayers = new List<OnlinePlayer>();
            foreach (var onlinePlayer in connection.OtherPlayers)
                mPlayers.Add(new(onlinePlayer.Key, onlinePlayer.Value.Item2, onlinePlayer.Value.Item1));
        }

        public void StartGame(int playerNumbersTurn)
        {
            Client.SendData(ClientSendMessages.RequestHand, Player.Number);
            mCurrentPlayerNumbersTurn = playerNumbersTurn;
        }

        internal void ImGuiUpdate()
        {
            Player.ImGuiDraw();
            foreach (var player in mPlayers) 
                player.ImGuiDraw();
        }
    }
}

