using ImGuiNET;
using System;
using System.Linq;

namespace MonopolyDeal
{
    public class PlayPlunderPopup : PlayActionCardPopup
    {
        OnlinePlayer mSelectedPlayer = null;
        int mSelectedIndex = 0;
        string[] mCardsInHand = [];
        public PlayPlunderPopup()
            : base(nameof(PlayPlunderPopup)) 
        { 
            AutoResize = true;
        }

        public override void Open(Card card)
        {
            int maxTimes = App.GetState<Gameplay>().PlayerManager.OnlinePlayers.Count;
            int times = 0;
            mSelectedIndex = 0;

            GetPlayerNames();

            while (mCardsInHand.Length == 0 && times++ <= maxTimes)
                GetCardsInHand();

            base.Open(card);
        }

        public override void ImGuiDraw()
        {
            AsMoneyLogic();

            if (SelectPlayer())
                GetCardsInHand();

            bool hasCards = mCardsInHand.Length > 0;

            if (hasCards)
                ImGui.Combo("Card To Steal", ref mSelectedIndex, mCardsInHand, mCardsInHand.Length);

            if (!hasCards)
                ImGui.BeginDisabled();

            if (ImGui.Button($"Steal {mSelectedPlayer.Name}'s Card {mCardsInHand[mSelectedIndex]}"))
            {
                var gameplay = App.GetState<Gameplay>();
                var localPlayer = gameplay.PlayerManager.LocalPlayer;
                PlunderDealValues values = new PlunderDealValues();
                values.targetPlayerNumber = mSelectedPlayer.Number;
                values.handIndex = mSelectedIndex;

                Client.SendData(ClientSendMessages.PlayPlunderCard, ref values, localPlayer.Number);

                gameplay.GetWindow<GettingDealWindow>()
                    .Open(localPlayer, mSelectedPlayer, $"You Have Plundered A Card From {mSelectedPlayer.Name}", localPlayer.Number);

                Close();
            }

            if (!hasCards)
                ImGui.EndDisabled();

            CloseLogic();
        }

        void GetCardsInHand()
        {
            var playerManager = App.GetState<Gameplay>().PlayerManager;
            mSelectedPlayer = playerManager.OnlinePlayers[mPlayerIndex];
            mCardsInHand = Enumerable.Range(1, mSelectedPlayer.CardsInHand).Select(x => x.ToString()).ToArray();
        }

    }
}