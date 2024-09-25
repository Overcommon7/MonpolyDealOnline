using System;
using System.IO;

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

            base.OnOpen();
        }

        public override void OnClose()
        {
            Client.mOnMessageRecieved -= Client_OnMessageRecieved;
            base.OnClose();
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
                    SystemMessageHandler.PropertyCardPlayed(PlayerManager, playerNumber, data);
                    break;
                case ServerSendMessages.MoneyCardPlayed:
                    SystemMessageHandler.MoneyCardPlayed(PlayerManager, playerNumber, data);
                    break;
                case ServerSendMessages.RentCardPlayed:                      
                    SystemMessageHandler.RentCardPlayed(GetWindow<PayPopup>(), PlayerManager, playerNumber, data);
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
                    if (PaymentHandler.PaymentInProcess)
                        PaymentHandler.OnPlayerSaidNo(GetWindow<GettingPaidWindow>(), PlayerManager, playerNumber);

                    break;

                case ServerSendMessages.NoWasRejected:
                    if (PaymentHandler.PaymentInProcess)
                        PaymentHandler.RejectedNo(PlayerManager, playerNumber);

                    break;
                case ServerSendMessages.PlayerPaid:
                    if (PaymentHandler.PaymentInProcess)
                        PaymentHandler.OnPlayerPaid(PlayerManager, playerNumber, data);

                    break;
                case ServerSendMessages.OnAllPlayersPaid:
                    PaymentHandler.OnAllPlayersPaid();
                    break;
                case ServerSendMessages.PaymentComplete:
                    PaymentHandler.PaymentComplete(PlayerManager.LocalPlayer, playerNumber);
                    break;
                case ServerSendMessages.CardMoved:
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
                case ServerSendMessages.OnTurnStarted:
                    PlayerManager.SetCurrentPlayer(playerNumber);
                    SystemMessageHandler.TurnStarted(PlayerManager, data);

                    if (PlayerManager.CurrentPlayerNumbersTurn == PlayerManager.LocalPlayer.Number)
                        State = State.PlayingCards;
                    else
                        State = State.NotTurn;
                    break;
                case ServerSendMessages.OnPlayerWin:
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
            AddWindow<GettingPaidWindow>();
            AddWindow<MoveCardPopup>();
            AddWindow<PayPopup>();
            AddWindow<PlayActionCardPopup>();
            AddWindow<PlayBuildingCardPopup>();
            AddWindow<PlayWildCardPopup>();            
            AddWindow<SlyDealPopup>();
            AddWindow<TooManyCardsPopup>();
            AddWindow<WildRentPopup>();
        }
    }
}
