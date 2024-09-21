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
        public PlayerManager PlayerManager { get; private set; }
        public int StartingPlayerNumber { get; set; }
        public State State { get; private set; }
        State mPreviousState = State.NotTurn;
        public override void OnOpen()
        {
            Client.mOnMessageRecieved += Client_OnMessageRecieved;

            PlayerManager = new PlayerManager();

            AddWindow<LocalPlayerWindow>(PlayerManager.LocalPlayer);           

            foreach (var player in PlayerManager.OnlinePlayes)
                AddWindow<OnlinePlayerWindow>(player);

            StartGame();
        }

        public void SetToRespondingState()
        {
            mPreviousState = State;
            State = State.RespondingToAction;
        }

        public void RevertState()
        {
            State = mPreviousState;
        }

        private void Client_OnMessageRecieved(ServerSendMessages message, int playerNumber, byte[] data)
        {
            switch (message)
            {
                case ServerSendMessages.WildRentPlayed:                    
                    break;
                case ServerSendMessages.WildCardPlayed:
                    SystemMessageHandler.WildCardPlayed(PlayerManager, playerNumber, data);
                    break;
                case ServerSendMessages.PropertyCardPlayed:
                    break;
                case ServerSendMessages.CardMoved:
                    break;
                case ServerSendMessages.RentCardPlayed:
                    break;
                case ServerSendMessages.SlyDealPlayed:
                    break;
                case ServerSendMessages.ForcedDealPlayed:
                    break;
                case ServerSendMessages.DealBreakerPlayed:
                    break;
                case ServerSendMessages.SingleTargetActionCard:
                    break;
                case ServerSendMessages.ActionCardPlayed:
                    break;
                case ServerSendMessages.JustSayNoPlayed:
                    break;
                case ServerSendMessages.CardsPayed:
                    break;
                case ServerSendMessages.PlayerPaidValues:
                    break;
                case ServerSendMessages.OnAllPlayersPaid:
                    break;
                case ServerSendMessages.CardsSent:
                    SystemMessageHandler.OnCardsRecieved(PlayerManager, data, playerNumber);
                    break;
                case ServerSendMessages.UpdateCardsInHand:
                    SystemMessageHandler.OnlinePlayerHandUpdate(PlayerManager, data, playerNumber);
                    break;
                case ServerSendMessages.HandReturned:
                    PlayerManager.LocalPlayer.OnHandReturned(playerNumber, data);
                    break;
                case ServerSendMessages.OnPlayerWin:
                    break;
                case ServerSendMessages.OnTurnStarted:
                    PlayerManager.SetCurrentPlayer(playerNumber);
                    SystemMessageHandler.TurnStarted(PlayerManager, data);

                    if (PlayerManager.CurrentPlayerNumbersTurn == PlayerManager.LocalPlayer.Number)
                        State = State.PlayingCards;
                    else
                        State = State.NotTurn;

                    break;
                case ServerSendMessages.OnPlayerConnected:
                    break;
                case ServerSendMessages.OnPlayerDisconnected:
                    break;
                case ServerSendMessages.OnPlayerReconnected:
                    break;
                case ServerSendMessages.DebugSendCard:
                    int cardID = int.Parse(Format.ToString(data));
                    if (CardData.TryGetCard<Card>(cardID, out var card))
                        PlayerManager.LocalPlayer.Hand.AddCard(card);
                    break;
            }
        }

        public void StartGame()
        {
            PlayerManager.StartGame(StartingPlayerNumber);

            if (PlayerManager.CurrentPlayerNumbersTurn == PlayerManager.LocalPlayer.Number)
                State = State.PlayingCards;
            else
                State = State.NotTurn;
        }

        public override void AddWindows()
        {
            AddWindow<ChargeRentPopup>();
            AddWindow<DealBreakerPopup>();
            AddWindow<ForcedDealPopup>();
            AddWindow<MoveCardPopup>();
            AddWindow<PayPopup>();
            AddWindow<PlayActionCardPopup>();
            AddWindow<PlayWildCardPopup>();
            AddWindow<SlyDealPopup>();
            AddWindow<TooManyCardsPopup>();
        }
    }
}
