﻿using System;
using System.Linq;
using ImGuiNET;

namespace MonopolyDeal
{
    public class PlayWildCardPopup : PlayerPopup
    {
        int mSelectedButton = 0;
        int mSelectedTypeIndex = 0;
        string[] mSetTypes;
        bool mAsMoney = false;
        SetType mSelectedSetType;
        public PlayWildCardPopup()
            : base(nameof(PlayWildCardPopup)) 
        {
            mSetTypes = [SetType.None.ToString()];
        }

        public override void ImGuiDraw()
        {
            if (mCard is WildPropertyCard)
                ImGui.Checkbox("Play As Money", ref mAsMoney);

            if (!mAsMoney)
            {
                if (mCard is WildPropertyCard property)
                {
                    if (ImGui.RadioButton($"{property.SetType1}##PU", ref mSelectedButton, 0))
                        mSelectedSetType = property.SetType1;

                    if (ImGui.RadioButton($"{property.SetType2}##PU", ref mSelectedButton, 1))
                        mSelectedSetType = property.SetType2;
                }
                else if (mCard is WildCard wild)
                {
                    if (ImGui.Combo("Set To Play On", ref mSelectedTypeIndex, mSetTypes, mSetTypes.Length))
                        mSelectedSetType = Enum.Parse<SetType>(mSetTypes[mSelectedTypeIndex]);
                }
            }
           
            if (ImGui.Button(mAsMoney ? "Use As Money##WildPopup" : "Play##WildPopup"))
            {
                if (!mAsMoney && mCard is WildPropertyCard)
                    PlayCard();
                else
                    PlayAsMoney();
            }
        }

        private void PlayAsMoney()
        {
            if (mCard is null)
                return;

            var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;
            player.PlayedCards.AddMoneyCard(mCard);
            player.Hand.RemoveCard(mCard);
            Client.SendData(ClientSendMessages.PlayMoneyCard, mCard.ID.ToString(), player.Number);
        }

        void PlayCard()
        {
            var wildCard = mCard as WildCard;
            wildCard.SetCurrentType(mSelectedSetType);
            PlayWildCard values = new PlayWildCard();
            values.setType = mSelectedSetType;
            values.cardID = wildCard.ID;

            var player = App.GetState<Gameplay>().PlayerManager.LocalPlayer;
            player.PlayedCards.AddPropertyCard(wildCard);
            player.Hand.RemoveCard(wildCard);

            Client.SendData(ClientSendMessages.PlayWildCard, ref values, player.Number);
            Close();
        }

        public void Open(LocalPlayer player, Card card)
        {
            int count = player.PlayedCards.SetTypesPlayed.Count;
            mSetTypes = new string[count];
            int index = 0;

            foreach (var setType in player.PlayedCards.SetTypesPlayed)
            {
                if (setType == SetType.None)
                    mSelectedTypeIndex = index;

                mSetTypes[index++] = setType.ToString();                
            }

            if (card is WildPropertyCard propertyCard)
                mSelectedSetType = propertyCard.SetType1;
            else if (card is WildCard wild)
                mSelectedSetType = player.PlayedCards.SetTypesPlayed.FirstOrDefault();

            mSelectedButton = 0;
            mAsMoney = false;
            base.Open(card);
        }
    }
}