using System.Collections.Generic;

public class Deck
{
    Stack<Card> cards;
    List<Card> fullDeck;
    List<Card> remainingCards;
    int decksToUse = 1;
    public Deck(int decksToUse = 1)
    {
        fullDeck = new();
        cards = new();
        remainingCards = new();
        this.decksToUse = decksToUse;
    }

    public void LoadCardsFromFile()
    {
        List<PropertyCardSaveValues> propertyCards = new();
        List<WildPropertySaveValues> wildPropertyCards = new();
        List<ActionCardSaveValues> actionCards = new();
        List<RentCardSaveValues> rentCards = new();
        List<ActionValues> actionValues = new();
        List<MoneyValues> moneyCards = new();

        XMLSerializer.Load(Files.PropertyCardData, ref propertyCards);
        XMLSerializer.Load(Files.WildPropertyCardData, ref wildPropertyCards);
        XMLSerializer.Load(Files.ActionCardData, ref actionCards);
        XMLSerializer.Load(Files.RentCardData, ref rentCards);
        XMLSerializer.Load(Files.ActionValues, ref actionValues);
        XMLSerializer.Load(Files.MoneyValues, ref moneyCards);

        for (int y = 0; y < decksToUse; ++y)
        {
            for (int i = 0; i < 2; i++)
            {
                fullDeck.Add(new BuildingCard(ActionType.Hotel));
                fullDeck.Add(new WildCard());
            }

            for (int i = 0; i < 3; i++)
            {
                fullDeck.Add(new WildRentCard());
                fullDeck.Add(new BuildingCard(ActionType.House));
            }

            foreach (var card in propertyCards)
            {
                PropertyCard propertyCard = new(card.setType, card.name, card.value);
                fullDeck.Add(propertyCard);
            }

            foreach (var card in wildPropertyCards)
            {
                bool repeat = card.propertyValues.setType == SetType.Orange || card.propertyValues.setType == SetType.Red;

                for (int i = 0; i < (repeat ? 2 : 1); ++i)
                {
                    WildPropertyCard wildPropertyCard = new(card.setType1, card.setType2, card.propertyValues.value);
                    fullDeck.Add(wildPropertyCard);
                }
            }

            foreach (var card in actionCards)
            {
                int amount = actionValues.Find(value => value.type == card.type).amount;
                for (int i = 0; i < amount; ++i)
                {
                    ActionCard actionCard = new(card.type, card.value);
                    fullDeck.Add(actionCard);
                }
            }

            foreach (var card in moneyCards)
            {
                int amount = moneyCards.Find(value => value.value == card.value).amount;
                for (int i = 0; i < amount; ++i)
                {
                    MoneyCard moneyCard = new(card.value);
                    fullDeck.Add(moneyCard);
                }

            }

            foreach (var card in rentCards)
            {
                for (int i = 0; i < 2; ++i)
                {
                    RentCard rentCard = new(card.setType1, card.setType2);
                    fullDeck.Add(rentCard);
                }
            }
        }

        Shuffle(fullDeck);
        foreach (var card in fullDeck)
        {
            var value = CardData.CreateNewCard<Card>(card.ID);
            if (value is not null)
                cards.Push(value);
        }
    }

    public Card RemoveCardFromDeck()
    {
        if (cards.TryPop(out Card? card))
        {
            return card;
        }

        ReloadCards();
        return RemoveCardFromDeck();
    }

    public List<Card> RemoveMultipleCardsFromDeck(int amount)
    {
        List<Card> cards = []; 
        for (int i = 0; i < amount; ++i)
            cards.Add(RemoveCardFromDeck());

        return cards;
    }

    public void AddCardToRemainingPile(Card card)
    {
        var value = CardData.CreateNewCard<Card>(card.ID);
        if (value is not null)
            remainingCards.Add(value);
    }

    private void ReloadCards()
    {
        Shuffle(remainingCards);
        cards.Clear();

        foreach (var card in remainingCards)
        {
            var value = CardData.CreateNewCard<Card>(card.ID);
            if (value is not null)
                cards.Push(value);
        }
            
        remainingCards.Clear();
    }

    void Shuffle(List<Card> source)
    {
        for (int i = 0; i < source.Count - 1; ++i)
        {
            var indexToSwap = Random.Shared.Next(i, source.Count);
            (source[i], source[indexToSwap]) = (source[indexToSwap], source[i]);
        }
    }
}
