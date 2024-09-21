using System;
using System.Linq;
using ImGuiNET;

namespace MonopolyDeal
{
    public class PlayWildCardPopup : PlayerPopup
    {
        int mSelectedButton = 0;
        int mSelectedTypeIndex = 0;
        string[] mSetTypes;
        SetType mSelectedSetType;
        public PlayWildCardPopup()
            : base(nameof(PlayWildCardPopup)) 
        {
            mSetTypes = [SetType.None.ToString()];
        }

        public override void ImGuiDraw()
        {
            if (mCard is WildPropertyCard property)
            {
                ImGui.RadioButton($"{property.SetType1}##PU", ref mSelectedButton, 0);
                ImGui.RadioButton($"{property.SetType2}##PU", ref mSelectedButton, 1);
            }
            else if (mCard is WildCard wild)
            {
                if (ImGui.Combo("Set To Play On", ref mSelectedTypeIndex, mSetTypes, mSetTypes.Length))
                    mSelectedSetType = Enum.Parse<SetType>(mSetTypes[mSelectedTypeIndex]);
            }

            if (ImGui.Button("Play"))
            {
                PlayCard();
            }
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

            mSelectedSetType = SetType.None;
            mSelectedButton = 0;
            base.Open(card);
        }
    }
}