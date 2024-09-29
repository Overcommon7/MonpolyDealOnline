using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public partial class LocalPlayer : Player
    {
        Hand mHand;
        readonly Dictionary<State, Func<Card, int, bool>[]> mLogic;
        public Hand Hand => mHand;
        public LocalPlayer(int playerNumber, ulong id, string name)
            : base(playerNumber, id, name) 
        { 
            mHand = new Hand();
            mLogic = new Dictionary<State, Func<Card, int, bool>[]>
            {
                { State.NotTurn, [NotTurn_PropertyLogic, NotTurn_BuildingCards, NotTurn_HandLogic, NotTurn_MoneyLogic] },
                { State.PlayingCards, [OnTurn_PropertyLogic, OnTurn_BuildingCards, OnTurn_HandLogic, OnTurn_MoneyLogic] },
                { State.RespondingToAction, [RespondToAction_PropertyLogic, RespondToAction_BuildingCards, RespondToAction_HandLogic, RespondToAction_MoneyLogic] }
            };

            mPayPopup = App.GetState<Gameplay>().GetWindow<PayPopup>();
        }

        public override void ImGuiDraw()
        {
            var extraLogic = mLogic[mGameplay.State];
            ImGui.SeparatorText("Hand");
            bool disabled = !HasPlaysRemaining;

            if (disabled)
                ImGui.BeginDisabled();

            mHand.ImGuiDraw(extraLogic[2]);

            if (disabled)
                ImGui.EndDisabled();

            ImGui.Spacing();
            ImGui.SeparatorText("Played Cards");
            ImGui.Spacing();
            PlayedCards.ImGuiDraw(extraLogic[0], extraLogic[1], extraLogic[3]);
        }

        public void OnHandReturned(int playerNumber, byte[] data)
        {
            if (playerNumber != Number)
                return;

            var cards = Format.ToString(data);
            mHand.AddCards(Serializer.GetCardsFromString<Card>(cards));     
        }
    }
}