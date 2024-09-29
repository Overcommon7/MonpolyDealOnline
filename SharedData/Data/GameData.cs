using System.Text;

public static class GameData
{
    public static int PICK_UP_AMOUNT_ON_TURN_START  = 2;
    public static int PICK_UP_AMOUNT_ON_GAME_START  = 5;
    public static int PICK_UP_AMOUNT_ON_HAND_EMPTY  = 5;

    public static int MAX_CARDS_IN_HAND  = 7;
    public static int MAX_PLAYS_PER_TURN  = 3;
                        
    public static int DEBT_COLLECTOR_AMOUNT = 5;
    public static int BIRTHDAY_AMOUNT = 2;
           
    public static int HOUSE_RENT_INCREASE = 3;
    public static int HOTEL_RENT_INCREASE = 4;
           
    public static int DOUBLE_RENT_MULTIPLIER = 2;

    public const int ALL_PLAYER_NUMBER = 99;
    public const int CARD_DATA_FILE_COUNT = 6;
    public const int CARD_DATA_SIZE_DIGITS = 5;
    public const int CARD_DATA_HEADER_SIZE = CARD_DATA_FILE_COUNT * CARD_DATA_SIZE_DIGITS;
    public static readonly SetType[] SET_TYPES = (SetType[])Enum.GetValues(typeof(SetType)); 

    public static string Serialize()
    {
        return new StringBuilder()
            .Append(PICK_UP_AMOUNT_ON_TURN_START).Append(',')
            .Append(PICK_UP_AMOUNT_ON_GAME_START).Append(',')
            .Append(PICK_UP_AMOUNT_ON_HAND_EMPTY).Append(',')
            .Append(MAX_CARDS_IN_HAND).Append(',')
            .Append(MAX_PLAYS_PER_TURN).Append(',')
            .Append(DEBT_COLLECTOR_AMOUNT).Append(',')
            .Append(BIRTHDAY_AMOUNT).Append(',')
            .Append(HOUSE_RENT_INCREASE).Append(',')
            .Append(DOUBLE_RENT_MULTIPLIER).ToString();
    }

    public static void Deserialize(string data)
    {
        var numbers = data.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        PICK_UP_AMOUNT_ON_TURN_START = int.Parse(numbers[0]);
        PICK_UP_AMOUNT_ON_GAME_START = int.Parse(numbers[1]);
        PICK_UP_AMOUNT_ON_HAND_EMPTY = int.Parse(numbers[2]);
        MAX_CARDS_IN_HAND = int.Parse(numbers[3]);
        MAX_PLAYS_PER_TURN = int.Parse(numbers[4]);
        DEBT_COLLECTOR_AMOUNT = int.Parse(numbers[5]);
        BIRTHDAY_AMOUNT = int.Parse(numbers[6]);
        DOUBLE_RENT_MULTIPLIER = int.Parse(numbers[7]);
    }
}
