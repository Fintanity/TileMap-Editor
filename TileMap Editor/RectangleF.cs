using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TileMap_Editor
{
    public class RectangleF
    {
        public float X, Y, Width, Height;

        public float Left { get => X; set => X = value; }
        public float Right { get => X + Width; set => X = value - Width; }
        public float Top { get => Y; set => Y = value; }
        public float Bottom { get => Y + Height; set => Y = value - Height; }

        public Vector2 Location { get => new Vector2(X, Y); set { X = value.X; Y = value.Y; } }

        public RectangleF() { }

        public RectangleF(Vector2 Position, float width, float height)
        {
            X = Position.X;
            Y = Position.Y;
            Width = width;
            Height = height;
        }

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(RectangleF rect)
        {
            if (rect.Left < Right && Left < rect.Right && rect.Top < Bottom)
            {
                return Top < rect.Bottom;
            }

            return false;
        }
    }
}
