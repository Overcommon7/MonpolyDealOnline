public class Card : Copy<Card>
{
    public string Name { get; private set; }
    public int Value { get; private set; }
    public int ID { get; private set; }

    public Card(string name, int value)
    {
        Name = name;
        Value = value;
        ID = Hashing.GetHash32(name);
    }
    public Card Copy()
    {
        return new Card(Name, Value);
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

