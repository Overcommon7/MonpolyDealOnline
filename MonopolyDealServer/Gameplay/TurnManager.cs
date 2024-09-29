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

    public static void StartTurn(Deck deck)
    {
        var player = CurrentPlayer;
        var cards = deck.RemoveMultipleCardsFromDeck(GameData.PICK_UP_AMOUNT_ON_TURN_START);
        player.AddCardsToHand(cards);

        var data = Format.Encode(Serializer.SerializeListOfCards(cards));
        Server.SendMessageToPlayers(ServerSendMessages.CardsSent, CurrentPlayerNumberTurn, data, player.Number);

        StringBuilder stringBuilder = new StringBuilder();
        foreach (var connectedPlayer in PlayerManager.ConnectedPlayers)
        {
            stringBuilder
                .Append(connectedPlayer.Number)
                .Append(',')
                .Append(connectedPlayer.CardsInHand)
                .Append('|');
        }

        Thread.Sleep(100);
        data = Format.Encode(stringBuilder.ToString().Remove(stringBuilder.Length - 1, 1));
        Server.BroadcastMessage(ServerSendMessages.OnTurnStarted, data, CurrentPlayerNumberTurn);
    }

    public static void StartGame(int startingPlayerNumber)
    {
        CurrentPlayerNumberTurn = startingPlayerNumber;
    }
}