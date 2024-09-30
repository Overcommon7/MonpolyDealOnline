using System;
using ImGuiNET;

namespace MonopolyDeal
{
    public class ForcedDealPopup : PlayActionCardPopup
    {
        PropertyCard? mCardToTake;
        PropertyCard? mCardToGive;

        OnlinePlayer? mSelectedPlayer = null;
        LocalPlayer? mPlayer = null;
        public ForcedDealPopup()
            : base(nameof(ForcedDealPopup))
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

                mPlayer.PlayedCards.DrawProperties(GivePropertyLogic, "GIVE", " - " + mPlayer.Name);
                mSelectedPlayer.PlayedCards.DrawProperties(TakePropertyLogic, "TAKE", " - " + mSelectedPlayer.Name);

                if (mCardToTake is not null)
                    ImGui.TextColored(mCardToTake.Color.ToVector4(), "Taking: " + mCardToTake.DisplayName());
                
                if (mCardToGive is not null)
                    ImGui.TextColored(mCardToGive.Color.ToVector4(), "Giving: " + mCardToGive.DisplayName());

                if (mCardToGive is null || mCardToTake is null)
                    ImGui.BeginDisabled();

                if (ImGui.Button("Force Deal##FDPU"))
                {
                    ForcedDealValues values = new();
                    values.givingToPlayerCardID = mCardToGive.ID;
                    values.takingFromPlayerCardID = mCardToTake.ID;
                    values.playerTradingWithNumber = mSelectedPlayer.Number;
                    values.givingSetType = mCardToGive.SetType;
                    values.takingSetType = mCardToTake.SetType;

                    if (mCard is not null)
                        mPlayer.Hand.RemoveCard(mCard);

                    ++mPlayer.PlaysUsed;
                    Client.SendData(ClientSendMessages.PlayForcedDeal, ref values, mPlayer.Number);
                    Close();

                    App.GetState<Gameplay>().GetWindow<GettingDealWindow>().Open(mPlayer, mSelectedPlayer,
                            $"Force Dealed {mSelectedPlayer.Name}'s {mCardToTake.DisplayName()} Property For Your {mCardToGive.DisplayName()}", 
                            mPlayer.Number);

                }

                if (mCardToGive is null || mCardToTake is null)
                    ImGui.EndDisabled();
            }

            CloseLogic();
        }
        bool GivePropertyLogic(Card card, int id)
        {
            if (card is not PropertyCard property)
                return false;

            ImGui.SameLine();

            if (mCardToGive == property)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Chosen");
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button($"Choose##{id}"))
                    mCardToGive = property;
            }    
           
            return false;
        }

        bool TakePropertyLogic(Card card, int id)
        {
            if (card is not PropertyCard property)
                return false;

            ImGui.SameLine();

            if (mCardToTake == property)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Chosen");
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button($"Choose##{id}"))
                    mCardToTake = property;
            }
                
            return false;
        }
        public override void Open(Card card)
        {
            var playerManager = App.GetState<Gameplay>().PlayerManager;
            mPlayer = playerManager.LocalPlayer;
            mPlayerIndex = 0;
            mSelectedPlayer = playerManager.OnlinePlayers[0];
            mTargetPlayerNumber = mSelectedPlayer.Number;

            GetPlayerNames();

            base.Open(card);
        }
    }
}