using System;
using ImGuiNET;
using Windows.System;

namespace MonopolyDeal
{
    public class WildRentPopup : PlayActionCardPopup
    {
        int mSelectedTypeIndex = 0;
        int mRentAmount = 0;
        bool mUseDoubleRent = false;
        bool mHasDoubelRent = false;
        string[] mSetTypes = [];
        SetType mSelectedSetType = SetType.None;
        public WildRentPopup() 
            : base(nameof(WildRentPopup))
        {
            
        }

        public override void ImGuiDraw()
        {
            AsMoneyLogic();

            if (!mAsMoney)
            {
                if (ImGui.Combo("Charging Set Type", ref mSelectedTypeIndex, mSetTypes, mSetTypes.Length))
                    mSelectedSetType = Enum.Parse<SetType>(mSetTypes[mSelectedTypeIndex]);

                SelectPlayer();

                ImGui.Checkbox("Use Double Rent##WRPU", ref mUseDoubleRent);

                int rent = mRentAmount;

                ImGui.Text($"Amount To be Paid M{rent}");
                ImGui.SameLine();
                if (ImGui.Button("Charge##WRPU"))
                    PlayRent();
            }
        }

        void PlayRent()
        {
            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;

            if (mCard is not null)
            {
                player.PlayedCards.AddMoneyCard(mCard);
                player.Hand.RemoveCard(mCard);
            }

            if (mUseDoubleRent)
            {
                if (player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DoubleRent, out var doubleRent))
                {
                    player.Hand.RemoveCard(doubleRent);
                }                
            }

            WildRentPlayValues values = new();
            values.withDoubleRent = mUseDoubleRent;
            values.cardsOwnedInSet = player.PlayedCards.GetNumberOfCardsInSet(mSelectedSetType);
            values.chargingSetType = mSelectedSetType;
            values.targetPlayerNumber = mTargetPlayerNumber;
            values.cardID = mCard.ID;

            ++player.PlaysUsed;
            Client.SendData(ClientSendMessages.PlayWildRentCard, ref values, player.Number);
        }

        public void Open(LocalPlayer player, Card card)
        {
            int count = player.PlayedCards.SetTypesPlayed.Count;
            mSetTypes = new string[count];

            int index = 0;
            foreach (var setType in player.PlayedCards.SetTypesPlayed)
            {
                mSetTypes[index] = setType.ToString();
                int amount = player.GetRentAmount(setType);

                if (amount > mRentAmount)
                {
                    mRentAmount = amount;
                    mSelectedTypeIndex = index;
                    mSelectedSetType = setType;
                }

                ++index;
            }

            mHasDoubelRent = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DoubleRent, out var doubleRent);

            GetPlayerNames();
            base.Open(card);
        }
    }
}