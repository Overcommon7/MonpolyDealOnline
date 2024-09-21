public class Program
{
    static void Main(string[] args)
    {
        Configuration configuration = new Configuration();
        configuration.mLobbySize = 2;
        configuration.mDecksToUse = 1;

        CardData.LoadFromFile();
        ConnectionHandler.Start();

        GameManager.Configuration = configuration;

        Server.Start();

        Console.WriteLine("Press Q To Close Server");
        while (true)
        {
            var key = Console.ReadKey().Key;
            if (key == ConsoleKey.Q)
                break;
        }

       
    }
}

