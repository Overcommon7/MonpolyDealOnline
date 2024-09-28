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
       
        if (values.withDoubleRent)
        {
            if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DoubleRent, out var action))
                return;

            player.RemoveCardFromHand(action);
            deck.AddCardToRemainingPile(action);
        }

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

    static void CardPlayedToDeck<T>(Deck deck, Player player, int cardID, byte[]? data, ServerSendMessages message, Action<T>? action = null) where T : Card
    {
        var card = CardData.CreateNewCard<T>(cardID);

        if (card is null)
            return;

        action?.Invoke(card);

        player.RemoveCardFromHand(card);
        deck.AddCardToRemainingPile(card);

        if (data is null)
            Server.BroadcastMessage(message, player.Number);
        else
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
            CardPlayedToPlayArea<ActionCard>(player, values.cardID, data, ServerSendMessages.ActionCardPlayed, (card) =>
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

    public static void SlyDealPlayed(Deck deck, Player player, byte[] data)
    {
        var values = Format.ToStruct<SlyDealValues>(data);
        var status = PlayerManager.TryGetPlayer(values.targetPlayerNumber, out var takingFrom);
        if (status != ConnectionStatus.Connected)
            return;

        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.SlyDeal, out var slyDeal))
            return;

        if (!CardData.TryGetCard<ActionCard>(values.cardID, out var card))
            return;

        takingFrom.RemoveCardFromPlayArea(card);
        player.AddCardToPlayArea(card);

        CardPlayedToDeck<ActionCard>(deck, player, slyDeal.ID, data, ServerSendMessages.SlyDealPlayed); 
    }

    public static void ForcedDealPlayed(Deck deck, Player player, byte[] data)
    {
        throw new NotImplementedException();
    }

    public static void DealBreakerPlayed(Deck deck, Player player, byte[] data)
    {
        var values = Format.ToStruct<DealBreakerValues>(data);
        var status = PlayerManager.TryGetPlayer(values.targetPlayerNumber, out var target);
        if (status != ConnectionStatus.Connected) 
            return;

        var cards = target.GetPropertyCardsInSet(values.setType);
        var buildingCards = target.GetBuildingCardsInSet(values.setType);
        var amountForSet = CardData.GetValues(values.setType).AmountForFullSet;

        if (cards.Length > amountForSet)
            Array.Sort(cards, CardData.SortAlgorithm);

        for (int i = 0; i < amountForSet; ++i)
        {
            target.RemoveCardFromPlayArea(cards[i]);
            player.AddCardToPlayArea(cards[i]);
        }

        foreach (var card in buildingCards)
        {
            target.RemoveCardFromPlayArea(card);
            player.AddCardToPlayArea(card);
        }

        if (CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.DealBreaker, out var dealBreaker))
        {
            deck.AddCardToRemainingPile(dealBreaker);
            player.RemoveCardFromHand(dealBreaker);
        }
            
                   
        Server.BroadcastMessage(ServerSendMessages.DealBreakerPlayed, data, player.Number);
    }

    public static void BirthdayPlayed(Deck deck, Player player)
    {
        if (!CardData.TryGetCard<ActionCard>(card => card.ActionType == ActionType.ItsMyBirthday, out var card))
            return;

        CardPlayedToDeck<ActionCard>(deck, player, card.ID, )
    }

    public static void BuildingCardPlayed(Player player, byte[] data)
    {
        
    }

    public static void WildRentPlayed(Deck deck, Player player, byte[] data)
    {
        throw new NotImplementedException();
    }

    public static void MoveCard(Player player, byte[] data)
    {
        throw new NotImplementedException();
    }
}