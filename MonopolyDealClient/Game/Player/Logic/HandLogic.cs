using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        void OnTurn_HandLogic(Card card)
        {
            if (card is ActionCard)
            {

            }

            if (card is PropertyCard)
            {
                if (card is WildCard)
                {

                }
            }
        }
        void RespondToAction_HandLogic(Card card)
        {

        }
        void NotTurn_HandLogic(Card card)
        {

        }
    }
}