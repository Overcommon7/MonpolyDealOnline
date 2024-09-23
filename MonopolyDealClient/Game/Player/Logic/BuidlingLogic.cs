namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        bool OnTurn_BuildingCards(Card card, int id)
        {
            if (card is not BuildingCard building)
                return false;

            return false;

        }

        bool RespondToAction_BuildingCards(Card card, int id)
        {
            if (card is not BuildingCard building)
                return false;

            return false;
        }

        bool NotTurn_BuildingCards(Card card, int id)
        {
            if (card is not BuildingCard building)
                return false;

            return false;
        }

    }
}