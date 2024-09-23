public class MoneyCard : Card, Copy<MoneyCard>
{
    public MoneyCard(int value)
        : base("M" + value, value) 
    { 

    }

    public new MoneyCard Copy()
    {
        return new MoneyCard(Value);
    }

    public override string ToString()
    {
        return $"{Name} - ID: {ID}";
    }

    public override string DisplayName()
    {
        return Name;
    }
}

