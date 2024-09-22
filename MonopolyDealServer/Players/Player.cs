using System;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

public class Player : IEquatable<Player>
{
    TcpClient? mClient;
    ulong mID;
    int mNumber;
    string mName;

    List<Card> mHand;
    List<Card> mPlayArea;
    
    public ulong ID => mID;
    public int Number => mNumber;
    public TcpClient Client => mClient ?? throw new ArgumentNullException(nameof(mClient));
    public bool IsReady { get; set; }
    public string Name { get => mName; set => mName = value; }
    public int CardsInHand => mHand.Count;
    public int CardsInPlayArea => mPlayArea.Count;

    public Player(TcpClient client, string name, int playerNumber)
    {
        mClient = client;
        mNumber = playerNumber;
        mName = name;

        mHand = new List<Card>();
        mPlayArea = new List<Card>();

        mID = client.GetID();
        if (mID == 0)
            mID = (ulong)playerNumber;
    }
    public bool Equals(Player? other)
    {
        if (other is null)
            return false;

        return mID == other.mID && mNumber == other.mNumber;
    }
    public void SetNewClient(TcpClient client)
    {
        CloseClient();
        mClient = client;
        mID = client.GetID();
    }
    public void CloseClient()
    {
        if (mClient is null)
            return;

        mClient.Dispose();
        mClient = null;
    }
    public void AddCardToHand(Card card)
    {
        lock (mHand)
            mHand.Add(card);
    }
    public void AddCardsToHand(IEnumerable<Card> cards)
    {
        lock (mHand)
            mHand.AddRange(cards);
    }
    public void AddCardToPlayArea(Card card)
    {
        lock (mPlayArea)
            mPlayArea.Add(card);
    }
    public bool RemoveCardFromHand(Card card)
    {
        int index = mHand.FindIndex(c => c.ID == card.ID);
        if (index == -1)
            return false;

        lock (mHand)
            mHand.RemoveAt(index);
        return true;
    }
    public bool RemoveCardFromPlayArea(Card card)
    {
        lock (mPlayArea)
            return mPlayArea.Remove(card);
    }
    public bool RemoveCardFromPlayArea(Predicate<Card> predicate)
    {
        int index = mPlayArea.FindIndex(predicate);
        if (index == -1)
            return false;

        lock (mPlayArea)
            mPlayArea.RemoveAt(index);
        return true;
    }
}