using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace MonopolyDeal
{
    public class MoveCardPopup : PlayerPopup
    {
        string[] mSetTypes = [];
        SetType mSetType = SetType.None;
        SetType mCurrentSetType = SetType.None;
        int mSetIndex = 0;

        public MoveCardPopup()
            : base(nameof(MoveCardPopup)) { }

        public override void ImGuiDraw()
        {
            ImGui.TextColored(mCard.Color.ToVector4(), mCard.DisplayName());
            
            if (ImGui.Combo("Set Types##MCPU", ref mSetIndex, mSetTypes, mSetTypes.Length))
                mSetType = Enum.Parse<SetType>(mSetTypes[mSetIndex]);

            if (mSetType != mCurrentSetType)
            {
                if (ImGui.Button("Move Card##MCPU"))
                {
                    var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;
                    MoveValues values = new MoveValues();
                    values.oldSetType = mCurrentSetType;
                    values.newSetType = mSetType;
                    values.cardID = mCard.ID;

                    if (mCard is WildPropertyCard)
                        values.cardType = MoveCardType.WildProperty;
                    else if (mCard is WildCard)
                        values.cardType = MoveCardType.WildCard;
                    else if (mCard is BuildingCard)
                        values.cardType = MoveCardType.Building;

                    Client.SendData(ClientSendMessages.MoveCard, ref values, player.Number);
                }
            }    

            if (ImGui.Button("Close##MCPU"))
            {
                Close();
            }
        }

        public override void Open(Card card)
        {
            if (card is WildCard wild)
                GetWildTypes(wild);

            if (card is BuildingCard building)
                GetBuildingTypes(building);

            base.Open(card);
        }

        void GetBuildingTypes(BuildingCard building)
        {
            mSetIndex = 0;
            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;
            List<string> types = new List<string>();

            bool isHouse = building.IsHouse;

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
            {
                mSetTypes = [.. types];
                mSetType = Enum.Parse<SetType>(mSetTypes[0]);
            }

            mCurrentSetType = building.CurrentSetType;
        }

        void GetWildTypes(WildCard wild)
        {
            var gameplay = App.GetState<Gameplay>();
            var player = gameplay.PlayerManager.LocalPlayer;

            int count = player.PlayedCards.SetTypesPlayed.Count;
            mSetTypes = new string[count];
            int index = 0;

            foreach (var setType in player.PlayedCards.SetTypesPlayed)
            {
                if (setType == SetType.None)
                    mSetIndex = index;

                mSetTypes[index++] = setType.ToString();
            }

            if (wild is WildPropertyCard propertyCard)
                mSetType = propertyCard.SetType1;
            else 
                mSetType = player.PlayedCards.SetTypesPlayed.FirstOrDefault();

            mSetIndex = 0;
            mCurrentSetType = wild.SetType;
        }
    }
}