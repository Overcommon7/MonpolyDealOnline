using System;

namespace MonopolyDeal
{
    public enum State
    {
        NotTurn,
        PlayingCards,
        RespondingToAction
    }
    public class Gameplay : Appstate
    {
        public PlayerManager? PlayerManager { get; private set; }
        public int StartingPlayerNumber { get; set; }
        public State State { get; private set; }
        
        public override void OnOpen()
        {
            Client.mOnMessageRecieved += Client_OnMessageRecieved;

            PlayerManager = new PlayerManager();

            AddWindow<LocalPlayerWindow>(PlayerManager.LocalPlayer);

            foreach (var player in PlayerManager.OnlinePlayes)
                AddWindow<OnlinePlayerWindow>(player);

            StartGame();
        }

        private void Client_OnMessageRecieved(ServerSendMessages message, int playerNumber, byte[] data)
        {
            switch (message)
            {
                case ServerSendMessages.OnTurnEnded:
                    break;
                case ServerSendMessages.CardPlayed:
                    break;
                case ServerSendMessages.CardMoved:
                    break;
                case ServerSendMessages.RentCardPlayed:
                    break;
                case ServerSendMessages.ActionCardPlayed:
                    break;
                case ServerSendMessages.CardsPayed:
                    break;
                case ServerSendMessages.OnAllRequestsComplete:
                    break;
                case ServerSendMessages.JustSayNoPlayed:
                    break;
                case ServerSendMessages.OnPlayerWin:
                    break;
                case ServerSendMessages.OnPlayerTurnStarted:
                    break;
                case ServerSendMessages.HandReturned:
                    PlayerManager.LocalPlayer.OnHandReturned(playerNumber, data);
                    break;
                case ServerSendMessages.AllPlayersHands:
                    break;
                case ServerSendMessages.OnPlayerConnected:
                    break;
                case ServerSendMessages.OnPlayerDisconnected:
                    break;
                case ServerSendMessages.OnPlayerReconnected:
                    break;
            }
        }

        public void StartGame()
        {
            PlayerManager.StartGame(StartingPlayerNumber);
        }

        public override void AddWindows()
        {
            
        }
    }
}
