using System;
using System.Collections.Generic;

namespace MonopolyDeal
{
    public class LocalPlayer : Player
    {
        Hand mHand;
        readonly Dictionary<State, Action<Card>> mLogic;
        public Hand Hand => mHand;
        public LocalPlayer(int playerNumber, ulong id, string name)
            : base(playerNumber, id, name) 
        { 
            mHand = new Hand();
            mLogic = new Dictionary<State, Action<Card>>();
            mLogic.Add()
        }

        void OnTurn_HandLogic(Card card)
        {

        }

        void RespondToAction_HandLogic()
        {

        }

        void OnTurn_PlayAreaLogic()
        {

        }

        void RespondToAction_PlayAreaLogic()
        {

        }

        void NotTurn_HandLogic()
        {

        }

        void NotTurn_PlayAreaLogic()
        {

        }

        public override void ImGuiDraw()
        {
            mHand.ImGuiDraw();
            
        }
    }
}