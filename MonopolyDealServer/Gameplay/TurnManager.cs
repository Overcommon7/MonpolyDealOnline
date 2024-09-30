using System.Text;

public static class TurnManager
{
    public static int CurrentPlayerNumberTurn { get; private set; }
    public static Player CurrentPlayer
    {
        get
        {
            var status = PlayerManager.TryGetPlayer(CurrentPlayerNumberTurn, out var player);
            if (status == ConnectionStatus.Connected)
                return player;

            throw new InvalidOperationException("Player Has Disconnected");
        }
    }

    public static bool EndTurn(Deck deck)
    {
        var player = CurrentPlayer;
        if (player.CardsInHand == 0)
        {
            var cards = deck.RemoveMultipleCardsFromDeck(GameData.PICK_UP_AMOUNT_ON_HAND_EMPTY);
            player.AddCardsToHand(cards);
            Server.BroadcastMessage(ServerSendMessages.CardsSent, Serializer.SerializeListOfCards(cards), player.Number);
            Thread.Sleep(100);
        }

        for (int i = 0; i < PlayerManager.TotalPlayers; i++)
        {
            ++CurrentPlayerNumberTurn;
            if (CurrentPlayerNumberTurn > PlayerManager.TotalPlayers)
                CurrentPlayerNumberTurn = 0;

            var status = PlayerManager.TryGetPlayer(CurrentPlayerNumberTurn, out player);
            if (status == ConnectionStatus.Connected)
                return true;
        }

        return false;
    }

    public static void StartTurn(Deck deck, int playerNumber, int cardsInHand)
    {
        var player = CurrentPlayer;
        var cards = deck.RemoveMultipleCardsFromDeck(GameData.PICK_UP_AMOUNT_ON_TURN_START);
        player.AddCardsToHand(cards);

        var data = Format.Encode(Serializer.SerializeListOfCards(cards));
        Server.SendMessageToPlayers(ServerSendMessages.CardsSent, CurrentPlayerNumberTurn, data, player.Number);

        Thread.Sleep(100);
        Server.BroadcastMessage(ServerSendMessages.OnTurnStarted, $"{playerNumber},{cardsInHand}", CurrentPlayerNumberTurn);
    }

    public static void StartGame(int startingPlayerNumber)
    {
        CurrentPlayerNumberTurn = startingPlayerNumber;
    }
}