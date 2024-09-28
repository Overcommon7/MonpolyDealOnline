using ImGuiNET;
using System.Threading.Tasks;

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
        
        RentCard? mRent = null;
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

            if (mHasDoubleRent)
            {
                ImGui.Checkbox("Use Double Rent##CPP", ref mUseDoubleRent);
                ImGui.Spacing();
            }
                                
            if ((mCardsOwnedInSetTwo > 0 || mCardsOwnedInSetOne > 0) && ImGui.Button("Play##CPP"))
            {
                RentPlayValues values = new RentPlayValues();
                values.withDoubleRent = mUseDoubleRent;
                values.cardsOwnedInSet = mRadioButton == 0 ? mCardsOwnedInSetOne : mCardsOwnedInSetTwo;
                values.chargingSetType = mRadioButton == 0 ? mRent.TargetType1 : mRent.TargetType2;
                values.cardID = mRent.ID;

                var gameplay = App.GetState<Gameplay>();
                var player = gameplay.PlayerManager.LocalPlayer;
                int cardsInSet = player.PlayedCards.GetNumberOfCardsInSet(values.chargingSetType);

                int rentAmount = CardData.GetRentAmount(values.chargingSetType, cardsInSet);
                if (mUseDoubleRent)
                {
                    rentAmount *= 2;
                    CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DoubleRent, out var card);
                    player.Hand.RemoveCard(card, false);
                }

                player.Hand.RemoveCard(mRent);
                PaymentHandler.BeginPaymentProcess(mPlayerNumber, rentAmount);                
                Client.SendData(ClientSendMessages.PlayRentCard, ref values, mPlayerNumber);

                Close();
                gameplay.GetWindow<GettingPaidWindow>().Open();
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
            mHasDoubleRent = player.Hand.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DoubleRent, out var doubleRent);

            mCardsOwnedInSetOne = player.PlayedCards.GetNumberOfCardsInSet(mRent.TargetType1);
            mCardsOwnedInSetTwo = player.PlayedCards.GetNumberOfCardsInSet(mRent.TargetType2);

            if (mCardsOwnedInSetOne == 0 && mCardsOwnedInSetTwo > 0)
                mRadioButton = 1;

            if (mCardsOwnedInSetOne == 0 && mCardsOwnedInSetTwo == 0)
                mRadioButton = 10;

            mPlayerNumber = player.Number;
        }

        public override void Close()
        {
            mRadioButton = 0;
            mCardsOwnedInSetOne = 0;
            mCardsOwnedInSetTwo = 0;
            mPlayerNumber = 0;
            mUseDoubleRent = false;
            mHasDoubleRent = false;
            mRent = null;
            base.Close();
        }
    }
}
