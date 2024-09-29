#pragma warning disable
public static class Files
{
    public static string SaveDataDirectory =
        Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName,
            "LocalLow\\Overcommon7\\MonopolyDeal\\");

    public static readonly string PropertyCardData = SaveDataDirectory + "PropertyCardData.xml";
    public static readonly string WildPropertyCardData = SaveDataDirectory + "WildPropertyCardData.xml";
    public static readonly string ActionCardData = SaveDataDirectory + "ActionCardData.xml";
    public static readonly string RentCardData = SaveDataDirectory + "RentCardData.xml";
    public static readonly string ActionValues = SaveDataDirectory + "ActionValues.xml";
    public static readonly string MoneyValues = SaveDataDirectory + "MoneyValues.xml";
    public static readonly string CardValues = SaveDataDirectory + "CardValues.xml";


    static Files()
    {
        if (!Directory.Exists(SaveDataDirectory))
            Directory.CreateDirectory(SaveDataDirectory);
    }
}