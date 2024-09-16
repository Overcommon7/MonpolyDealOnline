public class RentCard : ActionCard, Copy<RentCard>
{
    public SetType TargetType1 {get; private set; }   
    public SetType TargetType2 { get; private set;  }

    public bool Contains(SetType type)
        => TargetType1 == type || TargetType2 == type;

    public bool TryGetMatchingType(IReadOnlyList<Card> cards, out SetType type)
    {
        type = SetType.None;
        foreach (var card in cards)
        {
            if (card is not PropertyCard property)
                continue;

            if (!Contains(property.SetType))
                continue;

            type = property.SetType;
            return true;
        }

        return false;
    }
    public RentCard(SetType setType1, SetType setType2) 
        : base(ActionType.Rent, 1, true, true, false)
    {
        TargetType1 = setType1;
        TargetType2 = setType2;
    }

    public override string ToString()
    {
        return $"Rent - {TargetType1},{TargetType2} - M{Value} - ID: {ID}";
    }

    public new RentCard Copy()
    {
        return new RentCard(TargetType1, TargetType2);
    }
}