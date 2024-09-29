using ImGuiNET;
using System;
using System.Drawing;
using System.Linq;

namespace MonopolyDeal
{
    public class PlayActionCardPopup : PlayerPopup
    {
        protected TargetType mTargeType = TargetType.None;
        protected bool mAsMoney = false;
        protected int mPlayerIndex = 0;
        protected int mTargetPlayerNumber = 0;
        protected string[] mPlayerNames = Array.Empty<string>();
        protected int mPlayerCount;
        public PlayActionCardPopup()
            : this(nameof(PlayActionCardPopup)) { }

        protected PlayActionCardPopup(string title)
            : base(title) 
        {
            mIsPopup = false;
            mIsClosable = false;
        }

        public override void ImGuiDraw()
        {
            if (mCard is not ActionCard action)
                return;

            AsMoneyLogic();

            if (!mAsMoney)
            {
                if (action.ActionType == ActionType.DebtCollector)
                    DebtCollectorLogic();
                else if (action.ActionType == ActionType.ItsMyBirthday)
                    BirthdayLogic();
                else if (action.ActionType == ActionType.PassGo)
                    PassGoLogic();                
            }           
        }
        protected void CloseLogic()
        {
            if (ImGui.Button("Cancel##PACP"))
                Close();
        }
        protected void AsMoneyLogic()
        {
            ImGui.Checkbox("Play As Money##PACP", ref mAsMoney);

            if (mAsMoney)
            {
                if (ImGui.Button("As Money##PACP"))
                    PlayAsMoney();
            }
        }

        protected bool SelectPlayer()
        {
            if (!ImGui.Combo("Target Player", ref mPlayerIndex, mPlayerNames, mPlayerNames.Length))
                return false;

            var onlinePlayer = App.GetState<Gameplay>().PlayerManager.OnlinePlayers[mPlayerIndex];
            mTargetPlayerNumber = onlinePlayer.Number;

            return true;
        }
        void DebtCollectorLogic()
        {
            SelectPlayer();
            ImGui.Spacing();
            if (ImGui.Button("Play Debt Collector##PACP"))
            {                
                if (mCard is not null)
                {
                    var gameplay = App.GetState<Gameplay>();
                    var player = gameplay.PlayerManager.LocalPlayer;
                    player.Hand.RemoveCard(mCard);
                }
                    
                DebtCollectorValues values = new();
                values.targetPlayerNumber = mTargetPlayerNumber;
                values.actionType = ActionType.DebtCollector;
                SinglePlayerCharged(ClientSendMessages.PlayDebtCollector, values.actionType, ref values, Constants.DEBT_COLLECTOR_AMOUNT);
            }
        }

        void BirthdayLogic()
        {
            if (!ImGui.Button("Play Birthday##PACP"))
                return;

            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;

            if (mCard is not null)
                player.Hand.RemoveCard(mCard);

            Client.SendData(ClientSendMessages.PlayBirthdayCard, player.Number);
            PaymentHandler.BeginPaymentProcess(player.Number, 2);

            Close();
            gameplay.GetWindow<GettingPaidWindow>().Open();
        }

        void PassGoLogic()
        {
            if (!ImGui.Button("Play Pass Go##PACP"))
                return;

            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;

            if (mCard is not null)
                player.Hand.RemoveCard(mCard);

            Client.SendData(ClientSendMessages.RequestCards, "2", player.Number);
            Close();
        }
        
        protected void SinglePlayerCharged<T>(ClientSendMessages message, ActionType actionType, ref T values, int amountDue)
            where T : struct
        {
            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;
            PaymentHandler.BeginPaymentProcess(player.Number, amountDue);

            ++player.PlaysUsed;
            Client.SendData(message, ref values, player.Number);
            Close();
            gameplay.GetWindow<GettingPaidWindow>().Open();
        }
        protected void PlayAsMoney()
        {
            if (mCard is null) 
                return;

            PlayActionCardValues values = new PlayActionCardValues();
            values.asMoney = true;
            values.addToPlayArea = true;
            values.cardID = mCard.ID;

            var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;

            ++player.PlaysUsed;
            player.Hand.RemoveCard(mCard);
            player.PlayedCards.AddMoneyCard(mCard);

            Client.SendData(ClientSendMessages.PlayActionCard, ref values, player.Number);
            Close();
        }

        public void Open(Card card, TargetType targetType)
        {
            mTargeType = targetType;

            GetPlayerNames();

            if (card is ActionCard action)
            {
                if (action.ActionType == ActionType.DebtCollector
                || action.ActionType == ActionType.ItsMyBirthday
                || action.ActionType == ActionType.PassGo)
                {
                    mAsMoney = false;
                }
                else
                {
                    mAsMoney = true;
                }
            }            

            base.Open(card);
        }

        protected void GetPlayerNames()
        {
            var gameplay = App.GetState<Gameplay>();

            gameplay.GetWindow<LocalPlayerWindow>().IsDisabled = true;
            var onlinePlayers = gameplay.PlayerManager.OnlinePlayers;
            mPlayerIndex = 0;
            mTargetPlayerNumber = onlinePlayers[mPlayerIndex].Number;

            mPlayerNames = new string[onlinePlayers.Count];
            for (int i = 0; i < onlinePlayers.Count; i++)
                mPlayerNames[i] = onlinePlayers[i].Name;
        }

        public override void Close()
        {
            App.GetState<Gameplay>().GetWindow<LocalPlayerWindow>().IsDisabled = false;
            base.Close();
        }
    }
}
