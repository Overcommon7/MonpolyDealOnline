using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MonopolyDeal
{
    public partial class Camera : IDisposable
    {
        public ref float Zoom => ref camera.Zoom;
        public ref float Rotation => ref camera.Rotation;
        public ref Vector2 Target => ref camera.Target;
        public Color TintColor { get; set; }
        public Color ClearColor { get; set; }
        public Vector2 Resolution { get => new(renderTexture.Texture.Width, renderTexture.Texture.Height); set => SetResolution(value); }
        public Vector2 MousePosition { get => GetMousePosition(); }

        Camera2D camera;
        RenderTexture2D renderTexture;
        Vector2 renderSize;
        Vector2 renderPosition;
        Vector2 relativePosition;
        Vector2 virtualScreenSpaceRatio;
        Vector2 cachedMousePosition;
        float aspectRatio;
        float inverseAspectRatio;
        bool terminated;
        bool center;
        bool usingVirtualScreenSize;
        int id;
        bool isMousePositionCached;
        bool isActive;

        static List<Camera> cameras = new List<Camera>();
        static int ID = 0;

        public Camera(Vector2? renderPosition = null, Vector2? virtualScreenSize = null) 
        {
            camera = new(Vector2.Zero, Vector2.Zero, 0, 1);
            this.renderPosition = renderPosition ?? new Vector2(0, 0);
            ClearColor = Color.Black;
            TintColor = Color.White;
            id = ID++;
            isMousePositionCached = false;
            isActive = true;
            center = renderPosition is null;
            usingVirtualScreenSize = virtualScreenSize is not null;


            Vector2 screenSize = new(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            if (usingVirtualScreenSize)
            {
                virtualScreenSpaceRatio = virtualScreenSize.Value / screenSize;
                renderSize = virtualScreenSize.Value;
            }
            else renderSize = screenSize;

            renderTexture = Raylib.LoadRenderTexture((int)renderSize.X, (int)renderSize.Y);
            aspectRatio = renderSize.X / renderSize.Y;
            inverseAspectRatio = renderSize.Y / renderSize.X;

            if (center)
            {
                CenterRenderPosition();
            }
            else
            {
                Vector2 relativePosition = renderPosition.Value / screenSize;
                SetRenderPositionRelative(relativePosition);
            }

            cameras.Add(this);
        }

        ~Camera()
        {
            if (!terminated)
                Dispose();
        }

        public static void DrawFrameBuffer()
        {
            foreach (var camera in cameras)
            {
                if (!camera.isActive)
                    continue;

                Rectangle source = new(0, 0, camera.renderTexture.Texture.Width, -camera.renderTexture.Texture.Height);
                Rectangle dest = new(camera.renderPosition.X, camera.renderPosition.Y, camera.renderSize.X, camera.renderSize.Y);
                Raylib.DrawTexturePro(camera.renderTexture.Texture, source, dest, Vector2.Zero, 0, camera.TintColor);
            }
        }

        public static Camera GetCamera(int id)
        {
            var camera = cameras.Find(x => x.id == id);
            if (camera is null)
                throw new IndexOutOfRangeException();

            return camera;
        }

        public static Vector2 GetMousePosition(int id) => GetCamera(ID).GetMousePosition();
        public static bool IsMouseWithinBounds(int id) => GetCamera(ID).IsMouseWithinBounds();

        public void Dispose()
        {
            cameras.Remove(this);
            terminated = false;
        }
    }
}
