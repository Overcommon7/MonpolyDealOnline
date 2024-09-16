using System.Numerics;
using Raylib_cs;

namespace MonopolyDeal
{
    public partial class Camera
    {
        public void BeginDrawing()
        {
            Raylib.BeginTextureMode(renderTexture);
            Raylib.ClearBackground(ClearColor);
            Raylib.BeginMode2D(camera);
        }
        public void EndDrawing()
        {
            Raylib.EndMode2D();
            Raylib.EndTextureMode();    
        }
        public void Update(bool isWindowResized)
        {
            isMousePositionCached = false;

            
            int screenWidth;

            if (usingVirtualScreenSize)
            {
                renderSize.Y = virtualScreenSpaceRatio.Y * App.ScreenSize.Y;
                screenWidth = (int)(virtualScreenSpaceRatio.X * App.ScreenSize.X);
            }
            else
            {
                renderSize.Y = App.ScreenSize.Y;
                screenWidth = (int)App.ScreenSize.X;
            }

            renderSize.X = aspectRatio * renderSize.Y;

            if (renderSize.X > screenWidth)
            {
                renderSize.Y = inverseAspectRatio * screenWidth;
                renderSize.X = screenWidth;
            }

            if (center)
                CenterRenderPosition();
            else
                SetRenderPositionRelative(relativePosition);
        }
        public Vector2 TransformPoint(Vector2 position)
        {
            Vector2 point = position - renderPosition;
            point = Raylib.GetScreenToWorld2D(point, camera);

            Vector2 pixelSize = new(renderTexture.Texture.Width, renderTexture.Texture.Height);
            Vector2 ratio = pixelSize / renderSize;

            return point * ratio;
        }

        public Vector2 GetMousePosition()
        {
            if (isMousePositionCached)
                return cachedMousePosition;

            isMousePositionCached = true;
            cachedMousePosition = TransformPoint(Raylib.GetMousePosition());
            return cachedMousePosition;
        }

        public bool IsPointWithinBounds(Vector2 position)
        {
            Rectangle rectangle = new(renderPosition, renderSize);
            return Raylib.CheckCollisionPointRec(position, rectangle);
        }

        public bool IsMouseWithinBounds()
        {
            return IsPointWithinBounds(Raylib.GetMousePosition());
        }   

        public void SetRenderPosition(Vector2 screenPosition)
        {
            SetRenderPositionRelative(screenPosition / App.ScreenSize);
        }

        void SetResolution(Vector2 value)
        {
            if (value == Resolution) return;

            Raylib.UnloadRenderTexture(renderTexture);
            renderTexture = Raylib.LoadRenderTexture((int)value.X, (int)value.Y);
            renderSize = value;
            aspectRatio = renderSize.X / renderSize.Y;
            inverseAspectRatio = renderSize.Y / renderSize.X;
        }

        void SetRenderPositionRelative(Vector2 relativePosition)
        {
            this.relativePosition = Vector2.Clamp(relativePosition, Vector2.Zero, Vector2.One);
            renderPosition.X = this.relativePosition.X * App.ScreenSize.X;
            renderPosition.Y = this.relativePosition.Y * App.ScreenSize.Y;
        }

        void CenterRenderPosition()
        {
            if (renderSize.X >= App.ScreenSize.X)
            {
                renderPosition.X = 0;
            }
            else
            {
                renderPosition.X = (App.ScreenSize.X - renderSize.X) * 0.5f;
            }

            if (renderSize.Y >= App.ScreenSize.Y)
            {
                renderPosition.Y = 0;
            }
            else
            {
                renderPosition.Y = (App.ScreenSize.Y - renderSize.Y) * 0.5f;
            }
        }
    }
}
