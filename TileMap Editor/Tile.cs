using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TileMap_Editor
{
    public class Tile : Sprite
    {
        public Dictionary<string, byte> tilesAround = new Dictionary<string, byte>()
        {
            { "t", 0 },
            { "tr",0 },
            { "r", 0 },
            { "br", 0 },
            { "b", 0 },
            { "bl", 0 },
            { "l", 0 },
            { "tl", 0 }
        };
        public int _tileType = 2;

        readonly int tileSheetColumns = 12;
        private static Texture2D _tileTexture;
        Color tileColor = Color.White;
        Rectangle srcRect;

        public Tile(Vector2 spritePosition) : base(spritePosition)
        {
            _spriteBox = new RectangleF(spritePosition, TileMapEditor.tileSize, TileMapEditor.tileSize);
            srcRect = new Rectangle(TileMapEditor.baseTileSize * _tileType, 0, TileMapEditor.baseTileSize, TileMapEditor.baseTileSize);
        }

        public byte tileTypeValue()
        {
            byte total = 0;
            if (tilesAround["t"] == 1) { total += (1 << 0); }
            if (tilesAround["tr"] == 1) { total += (1 << 1); }
            if (tilesAround["r"] == 1) { total += (1 << 2); }
            if (tilesAround["br"] == 1) { total += (1 << 3); }
            if (tilesAround["b"] == 1) { total += (1 << 4); }
            if (tilesAround["bl"] == 1) { total += (1 << 5); }
            if (tilesAround["l"] == 1) { total += (1 << 6); }
            if (tilesAround["tl"] == 1) { total += (1 << 7); }
            return total;
        }

        public void SetSourceRect(int type)
        {
            if (type == -1)
            {
                srcRect = new Rectangle(1 * TileMapEditor.baseTileSize, 0 * TileMapEditor.baseTileSize, TileMapEditor.baseTileSize, TileMapEditor.baseTileSize);
                tileColor = Color.Black;
            }
            else if (type == -3)
            {
                type = TileMapEditor.tileTypes[tileTypeValue()];
                srcRect = new Rectangle((type % tileSheetColumns) * TileMapEditor.baseTileSize, (type / tileSheetColumns) * TileMapEditor.baseTileSize, TileMapEditor.baseTileSize, TileMapEditor.baseTileSize);
                tileColor = Color.Green;
            }
            else
            {
                srcRect = new Rectangle((type % tileSheetColumns) * TileMapEditor.baseTileSize, (type / tileSheetColumns) * TileMapEditor.baseTileSize, TileMapEditor.baseTileSize, TileMapEditor.baseTileSize);
            }
        }

        public static void LoadTileTexture(ContentManager myContent, string fileName)
        {
            myContent.RootDirectory = "Content";
            _tileTexture = myContent.Load<Texture2D>(fileName);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_tileTexture, _spritePosition, srcRect, tileColor, 0, Vector2.Zero, TileMapEditor.tileSize/64.0f, SpriteEffects.None, 0f);
        }
    }
}