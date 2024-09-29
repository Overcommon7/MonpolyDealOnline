﻿public static class Constants
{
    public const int PICK_UP_AMOUNT_ON_TURN_START = 2;
    public const int PICK_UP_AMOUNT_ON_GAME_START = 5;
    public const int PICK_UP_AMOUNT_ON_HAND_EMPTY = 5;
    public const int MAX_CARDS_IN_HAND = 7;
    public const int MAX_PLAYS_PER_TURN = 3;
    public const int ALL_PLAYER_NUMBER = 99;

    public const int CARD_DATA_FILE_COUNT = 6;
    public const int CARD_DATA_SIZE_DIGITS = 5;
    public const int CARD_DATA_HEADER_SIZE = CARD_DATA_FILE_COUNT * CARD_DATA_SIZE_DIGITS;

    public const int DEBT_COLLECTOR_AMOUNT = 5;
    public const int BIRTHDAY_AMOUNT = 2;

    public const int HOUSE_RENT_INCREASE = 3;
    public const int HOTEL_RENT_INCREASE = 4;

    public const int DOUBLE_RENT_MULTIPLIER = 2;

    public static readonly SetType[] SET_TYPES = (SetType[])Enum.GetValues(typeof(SetType)); 
}
