using System;
using System.IO;
using System.Linq;

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
            AddWindow<OnlinePlayersWindow>().SetPlayers(PlayerManager.OnlinePlayers.ToArray());

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
                    SystemMessageHandler.WildRentCardPlayed(this, playerNumber, data);
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
                case ServerSendMessages.BirthdayCardPlayed:
                    if (!PaymentHandler.PaymentInProcess)
                        SystemMessageHandler.BirthdayPlayed(this, playerNumber);
                    break;
                case ServerSendMessages.SlyDealPlayed:
                case ServerSendMessages.DealBreakerPlayed:
                case ServerSendMessages.ForcedDealPlayed:
                    if (DealHandler.IsDealInProgress)
                        break;

                    DealHandler.StartDeal(PlayerManager, message, playerNumber, data);                    
                    break;
                case ServerSendMessages.DealComplete:
                    DealHandler.DealComplete(this);
                    break;
                case ServerSendMessages.DebtCollectorPlayed:
                    if (PaymentHandler.PaymentInProcess)
                        break;

                    SystemMessageHandler.DebtCollectorPlayed(this, playerNumber, data);
                    break;
                case ServerSendMessages.ActionCardPlayed:
                    SystemMessageHandler.OnActionCardPlayed(PlayerManager, playerNumber, data);
                    break;
                case ServerSendMessages.JustSayNoPlayed:
                    if (PaymentHandler.PaymentInProcess)
                        PaymentHandler.OnPlayerSaidNo(PlayerManager, playerNumber);

                    if (DealHandler.IsDealInProgress)
                        DealHandler.SaidNoPlayed(PlayerManager, playerNumber);
                    break;
                case ServerSendMessages.NoWasRejected:
                    if (PaymentHandler.PaymentInProcess)
                        PaymentHandler.RejectedNo(this, playerNumber);

                    if (DealHandler.IsDealInProgress)
                        DealHandler.SaidNoPlayed(PlayerManager, playerNumber);
                break;
                case ServerSendMessages.PlayerPaid:
                    if (PaymentHandler.PaymentInProcess)
                        PaymentHandler.OnPlayerPaid(PlayerManager, playerNumber, data);

                break;
                case ServerSendMessages.OnAllPlayersPaid:
                    PaymentHandler.OnAllPlayersPaid();
                    break;
                case ServerSendMessages.PaymentComplete:
                    PaymentHandler.PaymentComplete(PlayerManager);
                    PaymentHandler.EndPayment(this);
                    break;
                case ServerSendMessages.CardMoved:
                    SystemMessageHandler.CardMoved(PlayerManager, data, playerNumber);
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
                    var player = PlayerManager.GetPlayer(playerNumber);
                    GetWindow<MessagePopup>().Open([$"Player {player.Name} Has Won"], true, () =>
                    {
                        App.ChangeState<Connection>();
                    });
                    break;
                case ServerSendMessages.OnPlayerConnected:
                    break;
                case ServerSendMessages.OnPlayerDisconnected:
                    break;
                case ServerSendMessages.OnPlayerReconnected:
                    break;
                case ServerSendMessages.DebugSendCard:
                    if (playerNumber == PlayerManager.LocalPlayer.Number)
                    {
                        int cardID = int.Parse(Format.ToString(data));
                        var card = CardData.CreateNewCard<Card>(cardID);
                        if (card is not null)
                            PlayerManager.LocalPlayer.Hand.AddCard(card);
                    }
                    else
                    {
                        var player = PlayerManager.GetOnlinePlayer(playerNumber);
                        ++player.CardsInHand;
                    }                  
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
            _ = AddWindow<ChargeRentPopup>();
            _ = AddWindow<DealBreakerPopup>();
            _ = AddWindow<ForcedDealPopup>();
            _ = AddWindow<GettingDealWindow>();
            _ = AddWindow<GettingPaidWindow>();
            _ = AddWindow<MessagePopup>();
            _ = AddWindow<MoveCardPopup>();
            _ = AddWindow<PayPopup>();
            _ = AddWindow<PlayActionCardPopup>();
            _ = AddWindow<PlayBuildingCardPopup>();
            _ = AddWindow<PlayWildCardPopup>();            
            _ = AddWindow<SlyDealPopup>();
            _ = AddWindow<TooManyCardsPopup>();
            _ = AddWindow<WildRentPopup>();
        }
    }
}
