using Raylib_cs;
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
    public byte[] ProfilePictureData { get; set; } = Array.Empty<byte>();

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
        mHand.Add(card);
    }
    public void AddCardsToHand(IEnumerable<Card> cards)
    {
        mHand.AddRange(cards);
    }
    public void AddCardToPlayArea(Card card)
    {
        mPlayArea.Add(card);
    }
    public void RemoveCardFromHand(Card card)
    {
        int index = mHand.FindIndex(c => c.ID == card.ID);
        if (index != -1)
            mHand.RemoveAt(index);
    }
    public void RemoveCardFromPlayArea(Card card)
    {
        if (mPlayArea.Remove(card))
            return;

        RemoveCardFromPlayArea(c => c.ID == card.ID);

    }
    public bool RemoveCardFromPlayArea(Predicate<Card> predicate)
    {
        int index = mPlayArea.FindIndex(predicate);
        if (index == -1)
            return false;

        mPlayArea.RemoveAt(index);
        return true;
    }
    public Card? GetCardFromPlayArea(int cardID)
    {
        return mPlayArea.Find(card => card.ID == cardID);
    }

    public Card? GetCardFromPlayArea(Predicate<Card> predicate)
    {
        return mPlayArea.Find(predicate);
    }
    public Card[] GetPropertyCardsInSet(SetType type)
    {
        return mPlayArea.Where(card =>
        {
            if (card is not PropertyCard property)
                return false;

            return property.SetType == type;

        }).ToArray();
    }

    public Card[] GetBuildingCardsInSet(SetType type)
    {
        return mPlayArea.Where(card =>
        {
            if (card is not BuildingCard building)
                return false;

            return building.CurrentSetType == type;

        }).ToArray();
    }

}