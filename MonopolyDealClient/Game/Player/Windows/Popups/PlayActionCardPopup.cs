using ImGuiNET;
using System;
using System.Linq;

namespace MonopolyDeal
{
    public class PlayActionCardPopup : PlayerPopup
    {
        protected TargetType mTargeType = TargetType.None;
        protected bool mAsMoney = false;
        protected int mPlayerIndex = 0;
        protected int mTargetPlayerNumber = 0;
        protected string mPlayerNames = string.Empty;
        public PlayActionCardPopup()
            : base(nameof(PlayActionCardPopup))
        {
            mIsPopup = false;
            mIsClosable = false;
        }

        public override void ImGuiDraw()
        {
            if (mCard is not ActionCard action)
                return;

            ImGui.Checkbox("Play As Money##PACP", ref mAsMoney);

            if (mAsMoney)
            {
                if (ImGui.Button("Play##PACP"))
                    PlayAsMoney();                
            }
            else
            {
                if (action.ActionType == ActionType.DebtCollector)
                    DebtCollectorLogic();
                else if (action.ActionType == ActionType.ItsMyBirthday)
                    BirthdayLogic();
                else if (action.ActionType == ActionType.PassGo)
                    PassGoLogic();
            }

            if (ImGui.Button("Cancel##PACP"))
                Close();
        }

        protected void SelectPlayer()
        {
            if (!ImGui.Combo("Target Player", ref mPlayerIndex, mPlayerNames))
                return;

            var onlinePlayer = App.GetState<Gameplay>().PlayerManager.OnlinePlayers[mPlayerIndex];
            mTargetPlayerNumber = onlinePlayer.Number;
        }
        void DebtCollectorLogic()
        {
            SelectPlayer();
            ImGui.Spacing();
            if (ImGui.Button("Play##PACP"))
            {
                ActionAgainstOne(ActionType.DebtCollector, 5);
            }
        }

        void BirthdayLogic()
        {
            if (!ImGui.Button("Play##PACP"))
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
            if (!ImGui.Button("Play##PACP"))
                return;

            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;

            if (mCard is not null)
                player.Hand.RemoveCard(mCard);

            Client.SendData(ClientSendMessages.RequestCards, "2", player.Number);
        }
        
        protected void ActionAgainstOne(ActionType actionType, int amountDue)
        {
            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;
            PaymentHandler.BeginPaymentProcess(player.Number, amountDue);

            ActionAgainstOne values = new();
            values.targetPlayerID = mTargetPlayerNumber;
            values.actionType = actionType;

            Client.SendData(ClientSendMessages.ActionAgainstOne, ref values, player.Number);
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

            player.Hand.RemoveCard(mCard);
            player.PlayedCards.AddMoneyCard(mCard);

            Client.SendData(ClientSendMessages.PlayActionCard, ref values, player.Number);
        }

        public void Open(Card card, TargetType targetType)
        {
            mTargeType = targetType;
            mAsMoney = false;

            var gameplay = App.GetState<Gameplay>();

            gameplay.GetWindow<LocalPlayerWindow>().IsDisabled = true;
            var onlinePlayers = gameplay.PlayerManager.OnlinePlayers;
            mPlayerIndex = 0;
            mTargetPlayerNumber = onlinePlayers[mPlayerIndex].Number;
            mPlayerNames = string.Join('0', onlinePlayers.Select(x => x.Name));

            base.Open(card);
        }

        public override void Close()
        {
            App.GetState<Gameplay>().GetWindow<LocalPlayerWindow>().IsDisabled = false;
            base.Close();
        }
    }
}
