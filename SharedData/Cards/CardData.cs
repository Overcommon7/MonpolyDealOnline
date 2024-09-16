public enum SetType
{
    None,
    Brown,
    LightBlue,
    Purple,
    Orange,
    Red,
    Yellow,
    Green,
    DarkBlue,
    Railroad,
    Utilities
}

public enum ActionType
{
    DealBreaker,
    JustSayNo,
    SlyDeal,
    ForcedDeal,    
    Rent,
    WildRent,
    ItsMyBirthday,
    DebtCollector,
    Hotel,
    House,
    DoubleRent,
    PassGo
}

public enum CardType
{
    Property,
    WildProperty,
    Rent,
    WildRent,
    Building,
    Action,
    Money
}

public enum MoveCardType
{
    Building,
    WildCard,
    WildProperty
}

public struct Rent
{
    public int cardsOwned;
    public int rentAmount;
}

public static class CardData
{
    public struct Values
    {
        public int AmountForFullSet;
        public Rent[] Prices;
    }

    static Dictionary<SetType, Values> cardValues = new();
    static Dictionary<Type, int> sortTypes = new();
    static List<Card> cards = new List<Card>();

    public static IReadOnlyDictionary<Type, int> SortTypes => sortTypes;

    public static Values GetValues(SetType type) => cardValues[type];
    public static int SortAlgorithm(Card a, Card b)
    {
        if (a is PropertyCard apCard && b is PropertyCard bpCard)
        {
            return apCard.SetType.CompareTo(bpCard.SetType);
        }

        if (a is ActionCard aaCard && b is ActionCard abCard)
        {
            return aaCard.ActionType.CompareTo(abCard.ActionType);
        }

        if (a is MoneyCard maCard && b is MoneyCard mbCard)
        {
            return maCard.Value.CompareTo(mbCard.Value);
        }

        

        return SortTypes[a.GetType()].CompareTo(SortTypes[b.GetType()]);
    }

    public static bool TryGetCard<T>(int cardID, out T card) where T : Card
    {
        int index = cards.FindIndex(value =>
        {
            if (value.ID != cardID)
                return false;

            return value is T;
        });

        if (index == -1)
        {
            card = null;
            return false;
        }

        card = (T)cards[index];
        return true;
    }

    public static bool TryGetCard<T>(Predicate<T> predicate, out T value) where T : Card
    {
        foreach (var card in cards)
        {
            if (card is not T typedCard)
                continue;

            if (!predicate(typedCard))
                continue;

            value = typedCard; 
            return true;
        }

        value = null;
        return false;
    }

    public static T CreateNewCard<T>(int cardID) where T : Card
    {
        if (!TryGetCard(cardID, out T card))
            throw new ArgumentException();

        if (card is not Copy<T> copyable)
            throw new NullReferenceException();

        return copyable.Copy();
    }

    public static void Initialize()
    {
        sortTypes = new Dictionary<Type, int>
        {
            { typeof(BuildingCard), 8 },
            { typeof(WildCard), 7 },
            { typeof(ActionCard), 6 },
            { typeof(WildRentCard), 5 },
            { typeof(RentCard), 4 },
            { typeof(MoneyCard), 3 },
            { typeof(WildPropertyCard), 2 },
            { typeof(PropertyCard), 1 },
            { typeof(Card), 0 }
        };

        List<PropertyCardSaveValues> propertyCards = new();
        List<WildPropertySaveValues> wildPropertyCards = new();
        List<ActionCardSaveValues> actionCards = new();
        List<MoneyValues> moneyCards = new();
        List<RentCardSaveValues> rentCards = new();
        List<CardValues> cardValues = new();

        XMLSerializer.Load(Files.PropertyCardData, ref propertyCards);
        XMLSerializer.Load(Files.WildPropertyCardData, ref wildPropertyCards);
        XMLSerializer.Load(Files.ActionCardData, ref actionCards);
        XMLSerializer.Load(Files.MoneyValues, ref moneyCards);
        XMLSerializer.Load(Files.RentCardData, ref rentCards);
        XMLSerializer.Load(Files.CardValues, ref cardValues);

        cards.Add(new BuildingCard(ActionType.House));
        cards.Add(new BuildingCard(ActionType.Hotel));
        cards.Add(new WildRentCard());
        cards.Add(new WildCard());

        foreach (var card in propertyCards)
        {
            PropertyCard propertyCard = new(card.setType, card.name, card.value);
            cards.Add(propertyCard);
        }

        foreach (var card in wildPropertyCards)
        {
            WildPropertyCard wildPropertyCard = new(card.setType1, card.setType2, card.propertyValues.value);
            cards.Add(wildPropertyCard);
        }

        foreach (var card in actionCards)
        {
            ActionCard actionCard = new(card.type, card.value, card.requiresOutsideAction, card.requiresAllPlayerAction, card.requiresOnePlayerAction);
            cards.Add(actionCard);
        }

        foreach (var card in moneyCards)
        {
            MoneyCard moneyCard = new(card.value);
            cards.Add(moneyCard);
        }

        foreach (var card in rentCards)
        {
            RentCard rentCard = new(card.setType1, card.setType2);
            cards.Add(rentCard);
        }

        foreach (var value in cardValues)
        {
            Values values = new Values();
            values.AmountForFullSet = value.amountForFullSet;
            values.Prices = value.prices;
            CardData.cardValues.Add(value.setType, values);
        }
    }
}

