public class PropertyCard : Card, Copy<PropertyCard>
{
    protected SetType setType = SetType.None;
    public SetType SetType { get => setType; }
    public int AmountForFullSet => CardData.GetValues(setType).AmountForFullSet;
    public bool IsPartOfFullSet { get; set; } = false;
    public Rent GetRent(int cardsOwnedInSet)
    {
        var prices = CardData.GetValues(setType).Prices;
        int index = Array.FindIndex(prices, price => price.cardsOwned == cardsOwnedInSet);

        if (index == -1)
        {
            Console.WriteLine($"The Price Index {index} Is Out Of Bounds");
            throw new IndexOutOfRangeException();
        }

        return prices[index];
    }

    public PropertyCard(SetType setType, string name, int value)
        : base(name, value)
    {
       this.setType = setType;
    }

    public new PropertyCard Copy()
    {
        return new PropertyCard(SetType, Name, Value);
    }

    public override string ToString()
    {
        return $"{Name} - {SetType} - M{Value} - ID: {ID}";
    }
}

