﻿using ImGuiNET;
using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public partial class LocalPlayer : Player
    {
        Hand mHand;
        readonly Dictionary<State, Action<Card>[]> mLogic;
        public Hand Hand => mHand;
        public LocalPlayer(int playerNumber, ulong id, string name)
            : base(playerNumber, id, name) 
        { 
            mHand = new Hand();
            mLogic = new Dictionary<State, Action<Card>[]>
            {
                { State.NotTurn, [NotTurn_PropertyLogic, NotTurn_BuildingCards, NotTurn_HandLogic] },
                { State.PlayingCards, [OnTurn_PropertyLogic, OnTurn_BuildingCards, OnTurn_HandLogic] },
                { State.RespondingToAction, [RespondToAction_PropertyLogic, RespondToAction_BuildingCards, RespondToAction_HandLogic] }
            };
        }

        public override void ImGuiDraw()
        {
            var extraLogic = mLogic[mGameplay.State];
            ImGui.SeparatorText("Hand");
            mHand.ImGuiDraw(extraLogic[2]);

            ImGui.SeparatorText("Played Cards");
            PlayedCards.ImGuiDraw(extraLogic[0], extraLogic[1]);
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