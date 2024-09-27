using System.Text;
using SimpleTCP;

public static class PlayerActions
{
    public static void OnHandRequested(Deck deck, Player player, byte[] data)
    {
        var cards = DataMarshal.GetCards(deck, Constants.PICK_UP_AMOUNT_ON_HAND_EMPTY);
        player.AddCardsToHand(cards);
        var message = Format.ToData(ServerSendMessages.HandReturned, Serializer.SerializeListOfCards(cards), player.Number);
        player.Client.GetStream().Write(message, 0, message.Length);
    }

    public static void CardPlayed(Deck deck, Player player, int cardID)
    {
        if (!CardData.TryGetCard(cardID, out Card card))
            return;

        player.RemoveCardFromHand(card);
        deck.AddCardToRemainingPile(card);
    }

    public static void PutCardsBack(Deck deck, Player player, byte[] data)
    {
        var cards = Serializer.GetCardsFromString<Card>(Format.ToString(data));
        foreach (var card in cards)
        {
            player.RemoveCardFromHand(card);
            deck.AddCardToRemainingPile(card);
        }

        Server.SendMessageExcluding(ServerSendMessages.UpdateCardsInHand, player.Number, Format.Encode(player.CardsInHand.ToString()), player.Number);       
    }

    public static void WildCardPlayed(Player player, byte[] data)
    {
        var wildCard = Format.ToStruct<PlayWildCard>(data);
        CardPlayedToPlayArea<WildCard>(player, wildCard.cardID, data, ServerSendMessages.WildCardPlayed, card => {
            card.SetCurrentType(wildCard.setType);
        });       
    }

    public static void RentCardPlayed(Deck deck, Player player, byte[] data)
    {
        PaymentManager.StartNewPayment(player, TargetType.All);
        var values = Format.ToStruct<RentPlayValues>(data);
        CardPlayedToDeck<RentCard>(deck, player, values.cardID, data, ServerSendMessages.RentCardPlayed);        
    }

    public static void PropertyCardPlayed(Player player, byte[] data)
    {
        int cardID = int.Parse(Format.ToString(data));
        CardPlayedToPlayArea<PropertyCard>(player, cardID, data, ServerSendMessages.PropertyCardPlayed);
    }

    public static void MoneyCardPlayed(Player player, byte[] data)
    {
        int cardID = int.Parse(Format.ToString(data));
        CardPlayedToPlayArea<MoneyCard>(player, cardID, data, ServerSendMessages.MoneyCardPlayed);
    }

    static void CardPlayedToPlayArea<T>(Player player, int cardID, byte[] data, ServerSendMessages message, Action<T>? action = null) where T : Card
    {
        var card = CardData.CreateNewCard<T>(cardID);

        if (card is null)
            return;

        action?.Invoke(card);

        player.RemoveCardFromHand(card);
        player.AddCardToPlayArea(card);
        
        Server.SendMessageExcluding(message, player.Number, data, player.Number);
    }

    static void CardPlayedToDeck<T>(Deck deck, Player player, int cardID, byte[] data, ServerSendMessages message, Action<T>? action = null) where T : Card
    {
        var card = CardData.CreateNewCard<T>(cardID);

        if (card is null)
            return;

        action?.Invoke(card);

        player.RemoveCardFromHand(card);
        deck.AddCardToRemainingPile(card);

        Server.SendMessageExcluding(message, player.Number, data, player.Number);
    }

    public static void OnCardsRequested(Deck deck, Player player, byte[] data)
    {
        int numberOfCards = int.Parse(Format.ToString(data));
        var cards = deck.RemoveMultipleCardsFromDeck(numberOfCards);

        var cardsAsString = Serializer.SerializeListOfCards(cards);
        Server.BroadcastMessage(ServerSendMessages.CardsSent, cardsAsString, player.Number);
    }

    public static void ActionCardPlayed(Deck deck, Player player, byte[] data)
    {
        var values = Format.ToStruct<PlayActionCardValues>(data);

        if (values.asMoney)
        {
            CardPlayedToPlayArea<ActionCard>(player, values.cardID, data, ServerSendMessages.MoneyCardPlayed, (card) =>
            {
                card.SetAsMoney(true);
            });
        }
        else
        {
            CardPlayedToDeck<ActionCard>(deck, player, values.cardID, data, ServerSendMessages.ActionCardPlayed, (card) =>
            {
                card.SetAsMoney(false);
            });
        }
       
    }

    public static void ActionAgainstOne(Deck deck, Player player, byte[] data)
    {
        var values = Format.ToStruct<ActionAgainstOne>(data);
        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == values.actionType, out var action))
            return;

        ServerSendMessages message = new ServerSendMessages();
        switch (values.actionType)
        {
            case ActionType.DealBreaker: message = ServerSendMessages.DealBreakerPlayed; break;
            case ActionType.SlyDeal: message = ServerSendMessages.SlyDealPlayed; break;
            case ActionType.ForcedDeal: message = ServerSendMessages.ForcedDealPlayed; break;
            case ActionType.DebtCollector: 
                message = ServerSendMessages.DebtCollectorPlayed;
                PaymentManager.StartNewPayment(player, TargetType.One);
                break;
        }

        CardPlayedToDeck<ActionCard>(deck, player, action.ID, data, message);
    }

    public static void SlyDealPlayed(Deck sDeck, Player player, byte[] data)
    {
        var values = Format.ToStruct<SlyDealValues>(data);
        var status = PlayerManager.TryGetPlayer(values.targetPlayerNumber, out var takingFrom);
        if (status != ConnectionStatus.Connected)
            return;

         
    }

    public static void ForcedDealPlayed(Deck sDeck, Player player, byte[] data)
    {
        throw new NotImplementedException();
    }

    public static void DealBreakerPlayed(Deck sDeck, Player player, byte[] data)
    {
        throw new NotImplementedException();
    }

    public static void BirthdayPlayed(Deck sDeck, Player player, byte[] data)
    {
        throw new NotImplementedException();
    }

    public static void BuildingCardPlayed(Player player, byte[] data)
    {
        throw new NotImplementedException();
    }

    public static void WildRentPlayed(Deck sDeck, Player player, byte[] data)
    {
        throw new NotImplementedException();
    }

    public static void MoveCard(Player player, byte[] data)
    {
        throw new NotImplementedException();
    }
}