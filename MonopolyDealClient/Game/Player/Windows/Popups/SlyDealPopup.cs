using System;
using ImGuiNET;
using Windows.System;

namespace MonopolyDeal
{
    public class SlyDealPopup : PlayActionCardPopup
    {
        PropertyCard? mCardToSteal = null;
        OnlinePlayer mSelectedPlayer = null;
        public SlyDealPopup() 
            : base(nameof(SlyDealPopup))
        {
            
        }

        public override void ImGuiDraw()
        {
            AsMoneyLogic();

            if (!mAsMoney)
            {
                if (SelectPlayer())
                {
                    var playerManager = App.GetState<Gameplay>().PlayerManager;
                    mSelectedPlayer = playerManager.OnlinePlayers[mPlayerIndex];
                }

                mSelectedPlayer.PlayedCards.DrawProperties(PropertyLogic, "SDPU");

                if (mCardToSteal is null)
                {
                    ImGui.BeginDisabled();
                }
                else
                {
                    ImGui.TextColored(mCardToSteal.Color.ToVector4(), "Stealing " + mCardToSteal.DisplayName());
                }
                   

                if (ImGui.Button("Steal##SPDU"))
                {
                    SlyDealValues values = new();
                    values.targetPlayerNumber = mSelectedPlayer.Number;
                    values.cardID = mCardToSteal.ID;
                    values.setType = mCardToSteal.SetType;

                    var gameplay = App.GetState<Gameplay>();
                    var player = gameplay.PlayerManager.LocalPlayer;
                    if (mCard is not null)
                        player.Hand.RemoveCard(mCard);

                    ++player.PlaysUsed;

                    Client.SendData(ClientSendMessages.PlaySlyDeal, ref values, player.Number);
                    Close();

                    gameplay.GetWindow<GettingDealWindow>().Open(player, mSelectedPlayer,
                            $"Sly Dealed {mSelectedPlayer.Name}'s {mCardToSteal.DisplayName()} property", player.Number);
                }

                if (mCardToSteal is null)
                    ImGui.EndDisabled();
            }

            CloseLogic();
        }

        bool PropertyLogic(Card card, int id)
        {
            if (card is not PropertyCard property)
                return false;

            ImGui.SameLine();
            if (ImGui.Button($"Choose##{id}"))
                mCardToSteal = property;

            return false;
        }

        public override void Open(Card card)
        {
            mCardToSteal = null;
            var playerManager = App.GetState<Gameplay>().PlayerManager;
            mPlayerIndex = 0;
            mSelectedPlayer = playerManager.OnlinePlayers[0];
            mTargetPlayerNumber = mSelectedPlayer.Number;

            GetPlayerNames();

            base.Open(card);
        }
    }
}