using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;

namespace TileMap_Editor
{
    public static class Camera
    {
        public static Matrix Transform { get; private set; }
        public static float bottomLeftTile = (Game1.screenHeight - TileMapEditor.tileSize);
        public static Vector3 Position = new(0, (Game1.screenHeight - TileMapEditor.tileSize), 0);
        public static int moveSpeed = 5;
        
        private static void moveCamera() 
        {
            if (EditorManager.state != "Paused")
            {
                int horizontalMovement =
                    (Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Left))
                    - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Right)))
                    * moveSpeed;

                int verticalMovement =
                    (Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Up))
                    - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Down)))
                    * moveSpeed;

                Position.X += (Position.X + horizontalMovement < 0) ? horizontalMovement : -Position.X;
                Position.Y += (Position.Y + verticalMovement > bottomLeftTile) ? verticalMovement : bottomLeftTile - Position.Y;

                if (Keyboard.GetState().IsKeyDown(Keys.Back))
                {
                    Position.X = 0;
                    Position.Y = bottomLeftTile;
                }
            }
        }

        private static void SetCamera()
        {
            Transform = Matrix.CreateTranslation(Position);
        }
        
        public static void Update()
        {
            moveCamera();
            SetCamera();
        }
    }
}
