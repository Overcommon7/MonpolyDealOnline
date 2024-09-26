public class Card : Copy<Card>
{
    public string Name { get; private set; }
    public int Value { get; private set; }
    public int ID { get; private set; }
    public CardColor Color { get; private set; }

    public Card(string name, int value, CardColor color)
    {
        Name = name;
        Value = value;
        Color = color;
        ID = Hashing.GetHash32(name);
    }
    public Card Copy()
    {
        return new Card(Name, Value, Color);
    }
    public override string ToString()
    {
        return $"{Name} - M{Value} - ID: {ID}";
    }
    public virtual string DisplayName() 
    {
        return Name;
    }
}

