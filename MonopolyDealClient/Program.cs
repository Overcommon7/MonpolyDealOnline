using Raylib_cs;

namespace MonopolyDeal
{
    internal class Program
    {
        public static int? DebugNumber = null;
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!int.TryParse(args[i], out int number))
                    continue;

                DebugNumber = number;
                break;
            }

           App.Run();
        }
    }
}