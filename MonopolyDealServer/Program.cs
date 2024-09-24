using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

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

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint | ConfigFlags.AlwaysRunWindow);
        Raylib.InitWindow(640, 480, "Server");
        Raylib.SetTargetFPS(30);

        rlImGui.Setup();

        while (!Raylib.WindowShouldClose())
        {           
            Raylib.BeginDrawing();
            {
                Raylib.ClearBackground(Color.Black);
                {

                }
                Raylib.DrawFPS(5, 5);

                rlImGui.Begin();
                {
                    if (GameManager.CurrentState == GameState.Lobby)
                        ConnectionHandler.ImGuiDraw();

                    if (GameManager.CurrentState == GameState.InGame)
                        GameManager.ImGuiDraw();
                }
                rlImGui.End();

            }
            Raylib.EndDrawing();

            Server.ProcessClientRequests();
        }

        Server.Close();

    }
}

