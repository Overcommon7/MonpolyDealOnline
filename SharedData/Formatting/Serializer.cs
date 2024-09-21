using System.Text;

public static class Serializer
{
    public static string SerializeListOfCards<T>(List<T> cards) where T : Card 
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(cards.Count).Append('|');

        foreach (T card in cards)
        {
            builder.Append(card.ID).Append(',');
        }

        return builder.Remove(builder.Length - 1, 1).ToString();
    }

    public static T[] GetCardsFromString<T>(string data) where T : Card
    {
        if (data[0] == '0')
            return Array.Empty<T>();

        var strs = data.Split('|');
        int count = int.Parse(strs[0]);
        T[] cards = new T[count];
        var IDs = strs[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < count; i++)
        {
            int id = int.Parse(IDs[i]);
            cards[i] = CardData.CreateNewCard<T>(id);
        }

        return cards;
    }

    public static int GetNumberOfCardsInString(string data)
    {
        if (data[0] == '0')
            return 0;

        int index = data.IndexOf('|');
        if (index == -1)
            return 0;

        var number = data.Substring(0, index);
        if (int.TryParse(number, out int value))
            return value;

        return 0;
    }


}