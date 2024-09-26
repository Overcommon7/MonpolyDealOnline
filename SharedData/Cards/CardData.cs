using System.Net.Sockets;
using System.Numerics;
using System.Text;

public enum SetType
{
    None = 0,
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

public struct CardColor
{
    public byte r;
    public byte g;
    public byte b;

    public CardColor(byte r, byte g, byte b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
}

public static class CardData
{
    public struct Values
    {
        public int AmountForFullSet;
        public Rent[] Prices;
    }

    static Dictionary<SetType, Values> cardValues = new();
    static Dictionary<ActionType, CardColor> actionColors;
    static Dictionary<SetType, CardColor> cardColors;
    static Dictionary<int, CardColor> moneyColors;
    static Dictionary<Type, int> sortTypes = new();
    static List<Card> cards = new List<Card>();

    public static IReadOnlyDictionary<Type, int> SortTypes => sortTypes;
    public static IReadOnlyList<Card> Cards => cards;
    public static Values GetValues(SetType type) => cardValues[type];
    public static CardColor GetCardColor(int value) => moneyColors[value];
    public static CardColor GetCardColor(SetType type) => cardColors[type];    
    public static CardColor GetCardColor(ActionType type) => actionColors[type];

    static CardData()
    {
        cardColors = new Dictionary<SetType, CardColor>
        {
            { SetType.None, new(255, 255, 255) },
            { SetType.Brown, new(124, 64, 59) },
            { SetType.DarkBlue, new(31, 98, 172) },
            { SetType.Green, new(38, 143, 71) },
            { SetType.LightBlue, new(154, 190, 218) },
            { SetType.Orange, new(210, 133, 55) },
            { SetType.Purple, new(186, 68, 162) },
            { SetType.Railroad, new(25, 26, 28) },
            { SetType.Red, new(207, 31, 36) },
            { SetType.Yellow, new(214, 224, 59) },
            { SetType.Utilities, new(203, 220, 183) }
        };

        moneyColors = new Dictionary<int, CardColor>
        {
            { 1, new(180, 189, 173) },
            { 2, new(206, 125, 31) },
            { 3, new(118, 159, 28) },
            { 4, new(0, 160, 255) },
            { 5, new(111, 46, 110) },
            { 10, new(174, 149, 63) },
        };

        actionColors = new Dictionary<ActionType, CardColor>
        {
            { ActionType.DealBreaker, new(144, 139, 195) },
            { ActionType.JustSayNo, new(118, 169, 202) },
            { ActionType.SlyDeal, new(183, 187, 191) },
            { ActionType.ForcedDeal, new(191, 192, 194) },
            { ActionType.WildRent, new(247, 45, 147) },
            { ActionType.ItsMyBirthday, new(185, 176, 177) },
            { ActionType.DebtCollector, new(191, 205, 205) },
            { ActionType.Hotel, new(214, 47, 59) },
            { ActionType.House, new(18, 102, 61) },
            { ActionType.DoubleRent, new(204, 204, 187) },
            { ActionType.PassGo, new(212, 212, 200) }
        };
    }
    public static Vector4 ToVector4(this CardColor color)
    {
        return new Vector4(color.r / 255f, color.g / 255f, color.b / 255f, 1f);
    }
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

    public static int GetRentAmount(SetType type, int cardsOwnedInSet)
    {
        if (!cardValues.TryGetValue(type, out var values))
            return 0;

        int index = Array.FindIndex(values.Prices, rent => rent.cardsOwned == cardsOwnedInSet);
        if (index == -1)
            return 0;

        return values.Prices[index].rentAmount;
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

    public static T? CreateNewCard<T>(int cardID) where T : Card
    {
        if (!TryGetCard(cardID, out T card))
            throw new ArgumentException();

        if (typeof(T) != typeof(Card))
        {
            if (card is not Copy<T> copyable)
                throw new NotImplementedException();

            return copyable.Copy();
        }
       
        if (card is ActionCard action)
        {
            if (action is BuildingCard buildingCard)
                return buildingCard.Copy() as T;

            if (action is WildRentCard wildRentCard)
                return wildRentCard.Copy() as T;

            if (action is RentCard rent)
                return rent.Copy() as T;

            return action.Copy() as T;
        }

        if (card is PropertyCard property)
        {
            if (property is WildCard wildCard)
            {
                if (property is WildPropertyCard wildProperty)
                {
                    return wildProperty.Copy() as T;
                }

                return wildCard.Copy() as T;
            }

            return property.Copy() as T;
        }

        if (card is MoneyCard money)
            return money.Copy() as T;

        return card.Copy() as T;

    }

    public static void LoadFromFile()
    {
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

        Initialize(propertyCards, wildPropertyCards, actionCards, moneyCards, rentCards, cardValues);
    }

    public static byte[] LoadToMemory()
    {        
        List<byte[]> dataWrapper =
        [
            File.ReadAllBytes(Files.PropertyCardData),
            File.ReadAllBytes(Files.WildPropertyCardData),
            File.ReadAllBytes(Files.ActionCardData),
            File.ReadAllBytes(Files.MoneyValues),
            File.ReadAllBytes(Files.RentCardData),
            File.ReadAllBytes(Files.CardValues)
        ];

        StringBuilder builder = new StringBuilder();
        byte[] rv = new byte[dataWrapper.Sum(a => a.Length)];
        int offset = 0;

        foreach (byte[] array in dataWrapper)
        {
            Buffer.BlockCopy(array, 0, rv, offset, array.Length);
            offset += array.Length;
            builder.Append(array.Length.ToString().PadRight(Constants.CARD_DATA_SIZE_DIGITS));
        }

        return Format.CombineByteArrays(Format.Encode(builder.ToString().PadRight(Constants.CARD_DATA_HEADER_SIZE)), rv, true);
    }

    public static void LoadFromData(byte[] externalData)
    {
        List<PropertyCardSaveValues> propertyCards = new();
        List<WildPropertySaveValues> wildPropertyCards = new();
        List<ActionCardSaveValues> actionCards = new();
        List<MoneyValues> moneyCards = new();
        List<RentCardSaveValues> rentCards = new();
        List<CardValues> cardValues = new();

        using (var stream = new MemoryStream(externalData))
        {
            using (var reader = new StreamReader(stream))
            {
                char[] data = new char[Constants.CARD_DATA_HEADER_SIZE];
                reader.ReadBlock(data, 0, Constants.CARD_DATA_HEADER_SIZE);
                string sizes = new(data);

               
                int index = 0;
                for (int i = 0; i < Constants.CARD_DATA_FILE_COUNT; i++)
                {
                    var intermediate = sizes.Substring(i * Constants.CARD_DATA_SIZE_DIGITS, Constants.CARD_DATA_SIZE_DIGITS).TrimEnd();
                    int size = int.Parse(intermediate);
                    char[] fileData = new char[size];
                    reader.ReadBlock(fileData, index, size);
                    var buffer = Encoding.UTF8.GetBytes(fileData);

                    switch (i)
                    {
                        case 0: XMLSerializer.LoadObjectFromXMLMemory(buffer, ref propertyCards); break;
                        case 1: XMLSerializer.LoadObjectFromXMLMemory(buffer, ref wildPropertyCards); break;
                        case 2: XMLSerializer.LoadObjectFromXMLMemory(buffer, ref actionCards); break;
                        case 3: XMLSerializer.LoadObjectFromXMLMemory(buffer, ref moneyCards); break;
                        case 4: XMLSerializer.LoadObjectFromXMLMemory(buffer, ref rentCards); break;
                        case 5: XMLSerializer.LoadObjectFromXMLMemory(buffer, ref cardValues); break;
                    }
                }
            }
        }

        Initialize(propertyCards, wildPropertyCards, actionCards, moneyCards, rentCards, cardValues);
    }

    public static void Initialize(
        List<PropertyCardSaveValues> propertyCards,
        List<WildPropertySaveValues> wildPropertyCards,
        List<ActionCardSaveValues> actionCards,
        List<MoneyValues> moneyCards,
        List<RentCardSaveValues> rentCards,
        List<CardValues> cardValues)
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
            ActionCard actionCard = new(card.type, card.value);
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

