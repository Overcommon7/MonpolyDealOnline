using Raylib_cs;
using System.Numerics;

namespace MonopolyDeal
{
    internal class Program
    {
        public static int? DebugNumber = null;
        static void Main(string[] args)
        {
            Vector2 position = new();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("--Number"))
                {
                    if (int.TryParse(args[++i], out int number))
                        DebugNumber = number;
                }

                if (args[i].Contains("Position"))
                {
                    float.TryParse(args[++i], out position.X);
                    float.TryParse(args[++i], out position.Y);
                }

            }

           App.Run(position);
        }
    }
}