using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace TileMap_Editor
{
    public class Sprite
    {
        protected Texture2D _spriteTexture;
        protected Vector2 _spritePosition;
        protected RectangleF _spriteBox;
        protected Color _spriteColour;

        public Vector2 Center { get => new Vector2(_spritePosition.X + (_spriteBox.Width / 2), _spritePosition.Y + (_spriteBox.Height / 2)); }

        public Sprite()
        { }

        public Sprite(Vector2 spritePosition)
        {
            _spritePosition = spritePosition;
        }

        public Sprite(Vector2 spritePosition, Color spriteColour)
        {
            _spriteColour = spriteColour;
            _spritePosition = spritePosition;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_spriteTexture, _spritePosition, _spriteColour);
        }

        public virtual void LoadContent(ContentManager myContent, string fileName)
        {
            myContent.RootDirectory = "Content";
            _spriteTexture = myContent.Load<Texture2D>(fileName);
            _spriteBox = new RectangleF(_spritePosition, _spriteTexture.Width, _spriteTexture.Height);
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public Color Colour
        {
            get { return _spriteColour; }
            set { _spriteColour = value; }
        }

        public Vector2 Position
        {
            get { return _spritePosition; }
            set { _spritePosition = value; }
        }

        public RectangleF SpriteBox
        {
            get { return _spriteBox; }
            set { _spriteBox = value; }
        }
    }
}
