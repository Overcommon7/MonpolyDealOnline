using System;
using System.Collections.Generic;
using ImGuiNET;

namespace MonopolyDeal
{
    public class DealBreakerPopup : PlayActionCardPopup
    {
        string[] mSetTypes;
        int mSetIndex = 0;
        SetType mSetType;
        public DealBreakerPopup()
            : base(nameof(DealBreakerPopup))
        {
            mSetTypes = [];
        }

        public override void ImGuiDraw()
        {
            ImGui.Checkbox("Play As Money", ref mAsMoney);
            AsMoneyLogic();

            if (mAsMoney)
            {
                if (SelectPlayer())
                    GetTypes();

                if (ImGui.Combo("Set", ref mSetIndex, mSetTypes, mSetTypes.Length))
                    mSetType = Enum.Parse<SetType>(mSetTypes[mSetIndex]);

                if (ImGui.Button($"Steal Set: {mSetType}"))
                {
                    DealBreakerValues values = new DealBreakerValues();
                    values.targetPlayerNumber = mTargetPlayerNumber;
                    values.setType = mSetType;

                    var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;

                    if (mCard is not null)
                        player.Hand.RemoveCard(mCard);

                    Client.SendData(ClientSendMessages.PlayDealBreaker, ref values, player.Number);
                    Close();
                }
            }    

            CloseLogic();
        }

        public override void Open(Card card)
        {
            mAsMoney = false;
            Open(card, TargetType.One);
        }

        void GetTypes()
        {
            mSetIndex = 0;
            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.GetOnlinePlayer(mTargetPlayerNumber);
            List<string> types = new List<string>();

            foreach (var type in Constants.SET_TYPES)
            {
                var count = player.PlayedCards.GetNumberOfCardsInSet(type);
                if (count >= CardData.GetValues(type).AmountForFullSet)
                    types.Add(type.ToString());
            }

            mSetTypes = [.. types];
        }
    }
}