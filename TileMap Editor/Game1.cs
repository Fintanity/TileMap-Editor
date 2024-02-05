using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace TileMap_Editor
{
    public class Game1 : Game
    {
        public static  GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static int screenWidth, screenHeight;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 704;

            screenWidth = _graphics.PreferredBackBufferWidth;
            screenHeight = _graphics.PreferredBackBufferHeight;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            TileMapEditor.LoadContent(Content);
            EditorManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            Camera.Update();
            TileMapEditor.Update();
            EditorManager.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Beige);

            _spriteBatch.Begin(transformMatrix: Camera.Transform);
            
            TileMapEditor.Draw(_spriteBatch);
            EditorManager.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}