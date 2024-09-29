using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace MonopolyDeal
{ 
    public static class Extentions
    {
        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static int GetRentAmount(this Player player, SetType setType, bool withDoubleRent = false)
        {
            int amountInSet = player.PlayedCards.GetNumberOfCardsInSet(setType);
            return player.GetRentAmount(setType, amountInSet, withDoubleRent);
        }

        public static int GetRentAmount(this Player player, SetType setType, int cardsInSet, bool withDoubleRent = false)
        {
            bool hasHouse = player.PlayedCards.HasHouse(setType);
            bool hasHotel = hasHouse && player.PlayedCards.HasHotel(setType);
            int amountInSet = player.PlayedCards.GetNumberOfCardsInSet(setType);

            int amount = CardData.GetRentAmount(setType, amountInSet, hasHouse, hasHotel);
            if (withDoubleRent)
                amount *= Constants.DOUBLE_RENT_MULTIPLIER;

            return amount;
        }
    }
}
