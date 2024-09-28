using System;
using System.Collections.Generic;
using ImGuiNET;
using Raylib_cs;

namespace MonopolyDeal
{
    public class PlayBuildingCardPopup : PlayerPopup
    {
        bool mAsMoney = false;
        int mSetIndex = 0;
        SetType mSetType;
        string[] mSetTypes = [];

        BuildingCard? mBuilding;
        public PlayBuildingCardPopup()
            : base(nameof(PlayBuildingCardPopup)) { }

        public override void ImGuiDraw()
        {
            if (mBuilding is null)
                return;

            AsMoneyLogic();

            if (!mAsMoney && mSetTypes.Length > 0)
                BuildingLogic();

            CloseLogic();
        }

        private void CloseLogic()
        {
            if (ImGui.Button("Close##PBCPU"))
                Close();
        }

        void AsMoneyLogic()
        {
            ImGui.Checkbox("Play As Money##PACP", ref mAsMoney);

            if (!mAsMoney)
                return;

            if (!ImGui.Button("As Money##PACP"))
                return;

            PlayActionCardValues values = new PlayActionCardValues();
            values.asMoney = true;
            values.addToPlayArea = true;
            values.cardID = mCard.ID;

            var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;

            player.Hand.RemoveCard(mCard);
            player.PlayedCards.AddMoneyCard(mCard);

            Client.SendData(ClientSendMessages.PlayActionCard, ref values, player.Number);
            Close();
        }

        void BuildingLogic()
        {
            if (ImGui.Combo("Set##PACP", ref mSetIndex, mSetTypes, mSetTypes.Length))
                mSetType = Enum.Parse<SetType>(mSetTypes[mSetIndex]);

            if (!ImGui.Button("Play House"))
                return;

            if (mCard is null)
                return;

            PlayBuildingCard values = new();
            values.setType = mSetType;
            values.buildingType = mBuilding.ActionType;

            var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;

            player.Hand.RemoveCard(mCard);
            player.PlayedCards.AddBuildingCard(mBuilding, mSetType);

            Client.SendData(ClientSendMessages.PlayBuildingCard, ref values, player.Number);
            Close();
        }

        public override void Open(Card card)
        {
            mBuilding = card as BuildingCard;
            base.Open(card);
        }

        void GetTypes()
        {
            mSetIndex = 0;
            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;
            List<string> types = new List<string>();

            bool isHouse = mBuilding.IsHouse;

            foreach (var type in Constants.SET_TYPES)
            {
                if (type == SetType.None)
                    continue;

                var count = player.PlayedCards.GetNumberOfCardsInSet(type);
                if (count >= CardData.GetValues(type).AmountForFullSet)
                {
                    if (isHouse)
                        types.Add(type.ToString());
                    else if (player.PlayedCards.HasHouse(type))
                        types.Add(type.ToString());
                }
                    
            }
            if (types.Count == 0)
                mSetTypes = [];
            else
                mSetTypes = [.. types];
        }
    }
}
