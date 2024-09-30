using ImGuiNET;
using System.Threading.Tasks;
using Windows.System;

namespace MonopolyDeal
{
    public class ChargeRentPopup : PlayerPopup
    {
        int mRadioButton = 0;
        int mCardsOwnedInSetOne = 0;
        int mCardsOwnedInSetTwo = 0;
        int mPlayerNumber = 0;
        bool mUseDoubleRent = false;
        bool mHasDoubleRent = false;
        bool mAsMoney = false;
        
        RentCard? mRent = null;
        LocalPlayer? mPlayer = null;
        public ChargeRentPopup() 
            : base(nameof(ChargeRentPopup))
        {

        }

        public override void ImGuiDraw()
        {
            if (mRent is null)
                return;

            if (mCardsOwnedInSetOne == 0)
                ImGui.BeginDisabled();

            ImGui.RadioButton(mRent.TargetType1.ToString(), ref mRadioButton, 0);

            if (mCardsOwnedInSetOne == 0)
                ImGui.EndDisabled();

            if (mCardsOwnedInSetTwo == 0)
                ImGui.BeginDisabled();

            ImGui.RadioButton(mRent.TargetType2.ToString(), ref mRadioButton, 1);

            if (mCardsOwnedInSetTwo == 0)
                ImGui.EndDisabled();

            ImGui.Spacing();

            ImGui.Checkbox("Use As Money##CPP", ref mAsMoney);
            if (mAsMoney && ImGui.Button("Play As Money##CPP"))
            {
                mPlayer.Hand.RemoveCard(mRent);
                mPlayer.PlayedCards.AddMoneyCard(mRent);

                Client.SendData(ClientSendMessages.PlayMoneyCard, mRent.ID.ToString(), mPlayer.Number);
                Close();
            }
            else if (!mAsMoney)
            {
                if (mHasDoubleRent)
                {
                    ImGui.Checkbox("Use Double Rent##CPP", ref mUseDoubleRent);
                    ImGui.Spacing();
                }

                int cardsInSet = mRadioButton == 0 ? mCardsOwnedInSetOne : mCardsOwnedInSetTwo;
                var setType = mRadioButton == 0 ? mRent.TargetType1 : mRent.TargetType2;

                int? rentAmount = mPlayer?.GetRentAmount(setType, cardsInSet, mUseDoubleRent);

                ImGui.Text($"Amount To be Paid M{rentAmount.GetValueOrDefault()}");

                if ((mCardsOwnedInSetTwo > 0 || mCardsOwnedInSetOne > 0) && ImGui.Button("Play##CPP"))
                {
                    RentPlayValues values = new RentPlayValues();
                    values.withDoubleRent = mUseDoubleRent;
                    values.cardsOwnedInSet = mRadioButton == 0 ? mCardsOwnedInSetOne : mCardsOwnedInSetTwo;
                    values.chargingSetType = mRadioButton == 0 ? mRent.TargetType1 : mRent.TargetType2;
                    values.cardID = mRent.ID;

                    if (mUseDoubleRent)
                    {
                        CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DoubleRent, out var card);
                        mPlayer.Hand.RemoveCard(card, false);
                    }

                    mPlayer.Hand.RemoveCard(mRent);
                    PaymentHandler.BeginPaymentProcess(mPlayerNumber, rentAmount.GetValueOrDefault());
                    ++mPlayer.PlaysUsed;
                    Client.SendData(ClientSendMessages.PlayRentCard, ref values, mPlayerNumber);

                    Close();
                    App.GetState<Gameplay>().GetWindow<GettingPaidWindow>().Open();
                }
            }           

            if (ImGui.Button("Close##CPP"))
            {
                Close();
            }
        }

        public void Open(LocalPlayer player, Card card)
        {
            mRent = card as RentCard;
            if (mRent is null)
                return;

            base.Open(card);

            mUseDoubleRent = false;
            mAsMoney = false;

            mHasDoubleRent = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DoubleRent, out var doubleRent);

            mCardsOwnedInSetOne = player.PlayedCards.GetNumberOfCardsInSet(mRent.TargetType1);
            mCardsOwnedInSetTwo = player.PlayedCards.GetNumberOfCardsInSet(mRent.TargetType2);

            if (mCardsOwnedInSetOne == 0 && mCardsOwnedInSetTwo > 0)
                mRadioButton = 1;

            if (mCardsOwnedInSetOne == 0 && mCardsOwnedInSetTwo == 0)
                mRadioButton = 10;

            mPlayerNumber = player.Number;
            mPlayer = player;

            App.GetState<Gameplay>().GetWindow<LocalPlayerWindow>().CanEndTurn = false;
        }

        public override void Close()
        {
            mRadioButton = 0;
            mCardsOwnedInSetOne = 0;
            mCardsOwnedInSetTwo = 0;
            mPlayerNumber = 0;
            mUseDoubleRent = false;
            mHasDoubleRent = false;
            mAsMoney = false;
            mRent = null;

            App.GetState<Gameplay>().GetWindow<LocalPlayerWindow>().CanEndTurn = true;

            base.Close();
        }
    }
}
