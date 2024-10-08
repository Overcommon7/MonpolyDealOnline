﻿using MonopolyDeal;
using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using rlImGui_cs;
using ImGuiNET;
using System.IO;
using System;

namespace MonopolyDeal
{
    public static class App
    {
        static int? mQueriedState = 0;
        static int mCurrentStateIndex = -1;
        static bool mShouldClose = false;
        static List<Appstate> mAppstates = new();
        static Appstate? mCurrentState => mAppstates[mCurrentStateIndex];
        static bool mValidAppstateLoaded => mCurrentStateIndex >= 0 && mCurrentStateIndex < mAppstates.Count && mCurrentState is not null;

        delegate void SetClipboardText(string text);  
        public static Vector2 ScreenSize { get; private set; } = new Vector2(1280, 720);
        public static void Run(Vector2 position)
        {
            Initialize(position);
            Update();
            Terminate();
        }

        public static T GetState<T>() where T : Appstate
        {
            foreach (var state in mAppstates)
            {
                if (state is T value)
                    return value;
            }

            throw new System.Exception("App state not loaded");
        }

        public static T ChangeState<T>()
        {
            for (int i = 0; i < mAppstates.Count; i++)
            {
                if (mAppstates[i] is not T value)
                    continue;

                mQueriedState = i;
                return value;
            }

            throw new System.Exception("App state not loaded");
        }

        private static void Initialize(Vector2 position)
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint | ConfigFlags.AlwaysRunWindow);
            Raylib.InitWindow((int)ScreenSize.X, (int)ScreenSize.Y, "Monopoly Deal");
            Raylib.SetTargetFPS(30);
            Raylib.SetExitKey(KeyboardKey.Null);

            if (position.X > 1 && position.Y > 1)
                Raylib.SetWindowPosition((int)position.X, (int)position.Y);

            rlImGui.Setup();           

            AddStates();

            foreach (var state in mAppstates)
                state.AddWindows();

            foreach (var state in mAppstates)
                state.Intialize();

            ImGui.LoadIniSettingsFromMemory(IniSettings.Data);
        }

        private static void AddStates()
        {
            mAppstates.Add(new Connection());
            mAppstates.Add(new Gameplay());
        }

        private static void Update()
        {
            Camera camera = new Camera();


            while (!Raylib.WindowShouldClose() && !mShouldClose)
            {
                Time.Update();

                bool windowResized = Raylib.IsWindowResized();
                if (windowResized)
                    ScreenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

                if (mQueriedState is not null)
                    HandleQueriedState();

                if (mValidAppstateLoaded)
                {
                    camera.Update(windowResized);
                    mCurrentState.Update();

                    camera.BeginDrawing();
                    {
                        mCurrentState.Draw();
                    }
                    camera.EndDrawing();
                }

                Raylib.BeginDrawing();
                {
                    Raylib.ClearBackground(Color.Black);
                    Camera.DrawFrameBuffer();
                    Raylib.DrawFPS(5, 5);

                    rlImGui.Begin(Time.DeltaTime);
                    mCurrentState.ImGuiUpdate();
                    rlImGui.End();

                }
                Raylib.EndDrawing();

                Client.ProcessIncomingRequests();
            }

            camera.Dispose();
        }

        private static void Terminate()
        {
            if (mValidAppstateLoaded)
                mCurrentState.OnClose();

            foreach (var states in mAppstates)
                states.Terminate();

            rlImGui.Shutdown();
            Raylib.CloseWindow();

            if (File.Exists("imgui.ini"))
                File.Delete("imgui.ini");
        }

        public static void Quit()
        {
            mShouldClose = true;
        }

        private static void HandleQueriedState()
        {
            if (mValidAppstateLoaded)
                mCurrentState.OnClose();

            mCurrentStateIndex = mQueriedState.Value;

            if (mValidAppstateLoaded)
                mCurrentState.OnOpen();

            mQueriedState = null;

        }

    }
}

