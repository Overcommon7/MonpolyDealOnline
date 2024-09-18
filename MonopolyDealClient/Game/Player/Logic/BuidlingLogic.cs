namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        void OnTurn_BuildingCards(Card card)
        {
            if (card is not BuildingCard building)
                return;

        }

        void RespondToAction_BuildingCards(Card card)
        {
            if (card is not BuildingCard building)
                return;

        }

        void NotTurn_BuildingCards(Card card)
        {
            if (card is not BuildingCard building)
                return;

        }

    }
}