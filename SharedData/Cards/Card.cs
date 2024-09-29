public class Card : Copy<Card>
{
    public string Name { get; private set; }
    public int Value { get; private set; }
    public int ID { get; private set; }
    public CardColor Color => mColor;
    protected CardColor mColor;
    public Card(string name, int value, CardColor color)
    {
        Name = name;
        Value = value;
        mColor = color;
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

