public class MoneyCard : Card, Copy<MoneyCard>
{
    public MoneyCard(int value)
        : base("M" + value, value, CardData.GetCardColor(value)) 
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

    public override string GetToolTip()
    {
        return "A Card Used For Monetary Value";
    }
}

