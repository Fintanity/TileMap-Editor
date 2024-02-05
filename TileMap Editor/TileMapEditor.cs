using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TileMap_Editor
{
    /*
     * Loading Tile Maps ✓
     * Zooming Out ✓ ✓
     * Save Location ✓
     * Binary Reading And Writing✓
     * + RLE???
     * Fix Notices
     * Undo/Redo
     * Tile You Can Walk through ✓
     * Test TileMap With Player
     * Change Camera Speed ✓
     * Change Save Location
     * Player Shouldn't be able to wall jump invisible tiles
     */

    public static class TileMapEditor
    {
        public static int baseTileSize = 64, tileSize = 64;
        public static Dictionary<byte, int> tileTypes = new Dictionary<byte, int>()
        {
            { 0, 1 }, { 255, 2 }, { 199, 3 }, { 124, 4 },
            { 31, 5 }, { 241 , 6 }, { 193, 7 }, { 7, 8 },
            { 112, 9 }, { 28, 10 }, { 85, 11 }, { 215, 12 },
            { 125, 13 }, { 95, 14 }, { 245, 15 }, { 247, 16 },
            { 223, 17 }, { 127, 18 }, { 253, 19 }, { 17, 20 },
            { 68, 21 }, { 1, 22 }, { 16, 23 }, { 4, 24 },
            { 64, 25}, { 65, 26 }, { 5, 27 }, { 20, 28 },
            { 80, 29 }, { 23, 30 }, { 209, 31 }, { 113, 32 },
            { 29, 33 }, { 116, 34 }, { 92, 35 }, { 197, 36 },
            { 71, 37 }, { 87, 38 }, { 213, 39 }, { 117, 40 },
            { 93, 41 },{ 221, 42 }, { 119, 43 }, { 81, 44 },
            { 21, 45 }, { 84 ,46 }, { 69, 47 }
        };
        public static Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
        public static float arrayX = 0, arrayY = 0;

        static int drawSize = 1;
        static List<int> specialTiles = new List<int>() { 0, -1 };
        static List<Point> addedTiles = new List<Point>(), removedTiles = new List<Point>();
        static bool editedTiles = false;
        static Texture2D placeTileTexture;
        static Dictionary<string, Vector2> tilesAroundTile = new Dictionary<string, Vector2>()
        {
            { "t", new Vector2(0, 1) },
            { "tr", new Vector2(1, 1) },
            { "r", new Vector2(1, 0) },
            { "br", new Vector2(1, -1) },
            { "b", new Vector2(0, -1) },
            { "bl", new Vector2(-1, -1) },
            { "l", new Vector2(-1, 0) },
            { "tl", new Vector2(-1, 1) }
        };

        static Vector2 placeTilePosition()
        {
            MouseState mouseState = Mouse.GetState();
            return new Vector2((((int)(mouseState.X - Camera.Position.X) / tileSize) * tileSize), (float)((Math.Floor((mouseState.Y - Camera.Position.Y) / tileSize)) * tileSize));
        }

        static void CheckAroundTileAt(string tilePositionKey)
        {
            foreach (KeyValuePair<string, Vector2> kvp in tilesAroundTile)
            {
                int checkTileX = (int)(tiles[tilePositionKey].Position.X + (kvp.Value.X * tileSize));
                int checkTileY = (int)(tiles[tilePositionKey].Position.Y + (kvp.Value.Y * tileSize));

                if (kvp.Key.Length == 2)
                {
                    int allowCorner = 0;

                    switch (kvp.Key[0])
                    {
                        case 't':
                            if (tiles.ContainsKey($"{checkTileX},{checkTileY - tileSize}"))
                            {
                                if (!specialTiles.Contains(tiles[$"{checkTileX},{checkTileY - tileSize}"]._tileType)) allowCorner++;
                            }
                            break;
                        case 'b':
                            if (tiles.ContainsKey($"{checkTileX},{checkTileY + tileSize}"))
                            {
                                if (!specialTiles.Contains(tiles[$"{checkTileX},{checkTileY + tileSize}"]._tileType)) allowCorner++;
                            }
                            break;
                    }

                    switch (kvp.Key[1])
                    {
                        case 'l':
                            if (tiles.ContainsKey($"{checkTileX + tileSize},{checkTileY}"))
                            {
                                if (!specialTiles.Contains(tiles[$"{checkTileX + tileSize},{checkTileY}"]._tileType)) allowCorner++;
                            }
                            break;
                        case 'r':
                            if (tiles.ContainsKey($"{checkTileX - tileSize},{checkTileY}"))
                            {
                                if (!specialTiles.Contains(tiles[$"{checkTileX - tileSize},{checkTileY}"]._tileType)) allowCorner++;
                            }
                            break;
                    }

                    if (allowCorner == 2)
                    {
                        tiles[tilePositionKey].tilesAround[kvp.Key] = (tiles.ContainsKey($"{checkTileX},{checkTileY}") && !specialTiles.Contains(tiles[$"{checkTileX},{checkTileY}"]._tileType)) ? (byte)1 : (byte)0;
                    }
                    else
                    {
                        tiles[tilePositionKey].tilesAround[kvp.Key] = 0;
                    }
                }
                else
                {
                    tiles[tilePositionKey].tilesAround[kvp.Key] = (tiles.ContainsKey($"{checkTileX},{checkTileY}") && !specialTiles.Contains(tiles[$"{checkTileX},{checkTileY}"]._tileType)) ? (byte)1 : (byte)0;
                }
            }
        }

        public static Tile[,] ToTileArray()
        {

            Tile[,] tileArrayWithTiles = new Tile[(int)arrayX + 1, (int)Math.Abs(arrayY) + 1];

            foreach (Tile t in tiles.Values)
            {
                tileArrayWithTiles[(int)(t.Position.X / tileSize), (int)(Math.Abs(t.Position.Y) / tileSize)] = t;
            }
            return tileArrayWithTiles;
            
        }

        public static int[,] ToArray()
        {
            int[,] tileArray = new int[(int)arrayX + 1, (int)Math.Abs(arrayY) + 1];

            foreach (Tile t in tiles.Values)
            {
                tileArray[(int)(t.Position.X / tileSize), (int)(Math.Abs(t.Position.Y) / tileSize)] = (t._tileType == 0) ? 1 : ((t._tileType == -1) ? 3 : ((t._tileType == -3) ? 4 : 2));
            }
            return tileArray;
        }

        public static void ConfigureTileTypes(bool configureAll = true) 
        {
            if (configureAll)
            {
                foreach (string tilePos in tiles.Keys)
                {
                    if (tilePos != null)
                    {
                        CheckAroundTileAt(tilePos);
                        if (!specialTiles.Contains(tiles[tilePos]._tileType) && tiles[tilePos]._tileType != -3)
                        {
                            tiles[tilePos]._tileType = tileTypes[tiles[tilePos].tileTypeValue()];
                        }

                        if (tiles[tilePos]._tileType == -3)
                        {
                            tiles[tilePos].SetSourceRect(-3);
                        }
                        else
                        {
                            tiles[tilePos].SetSourceRect(tiles[tilePos]._tileType);
                        }
                    }
                }
            }
            else
            {
                foreach (Point Pos in addedTiles)
                {
                    string tilePos = $"{Pos.X},{Pos.Y}";

                    CheckAroundTileAt(tilePos);
                    if (!specialTiles.Contains(tiles[tilePos]._tileType) && tiles[tilePos]._tileType != -3)
                    {
                        tiles[tilePos]._tileType = tileTypes[tiles[tilePos].tileTypeValue()];
                    }

                    if (tiles[tilePos]._tileType == -3)
                    {
                        tiles[tilePos].SetSourceRect(-3);
                    }
                    else
                    {
                        tiles[tilePos].SetSourceRect(tiles[tilePos]._tileType);
                    }

                    foreach ( Vector2 v in tilesAroundTile.Values)
                    {
                        string checkTilePos = $"{Pos.X + (v.ToPoint().X * tileSize)},{Pos.Y + (v.ToPoint().Y * tileSize)}";
                        if (tiles.ContainsKey(checkTilePos))
                        {
                            CheckAroundTileAt(checkTilePos);
                            if (!specialTiles.Contains(tiles[checkTilePos]._tileType) && tiles[checkTilePos]._tileType != -3)
                            {
                                tiles[checkTilePos]._tileType = tileTypes[tiles[checkTilePos].tileTypeValue()];
                            }

                            if (tiles[checkTilePos]._tileType == -3)
                            {
                                tiles[checkTilePos].SetSourceRect(-3);
                            }
                            else
                            {
                                tiles[checkTilePos].SetSourceRect(tiles[checkTilePos]._tileType);
                            }
                        }
                    }
                }
                foreach (Point Pos in removedTiles)
                {
                    foreach (Vector2 v in tilesAroundTile.Values)
                    {
                        string checkTilePos = $"{Pos.X + (v.ToPoint().X * tileSize)},{Pos.Y + (v.ToPoint().Y * tileSize)}";
                        if (tiles.ContainsKey(checkTilePos))
                        {
                            CheckAroundTileAt(checkTilePos);
                            if (!specialTiles.Contains(tiles[checkTilePos]._tileType) && tiles[checkTilePos]._tileType != -3)
                            {
                                tiles[checkTilePos]._tileType = tileTypes[tiles[checkTilePos].tileTypeValue()];
                            }

                            if (tiles[checkTilePos]._tileType == -3)
                            {
                                tiles[checkTilePos].SetSourceRect(-3);
                            }
                            else
                            {
                                tiles[checkTilePos].SetSourceRect(tiles[checkTilePos]._tileType);
                            }
                        }
                    }
                }

                addedTiles.Clear();
                removedTiles.Clear();
            }
        }

        public static void Update()
        {
            if (EditorManager.state != "Paused")
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Vector2 newTilePosition;
                    string tileKeyName;
                    for (int i = 0; i < drawSize; i++)
                    {
                        for (int j = 0; j < drawSize; j++)
                        {
                            newTilePosition = placeTilePosition() + new Vector2(i*tileSize, j*tileSize);
                            tileKeyName = $"{newTilePosition.X},{newTilePosition.Y}";

                            if (newTilePosition.X >= 0 && newTilePosition.Y <= 0)
                            {
                                if (!tiles.ContainsKey(tileKeyName))
                                {
                                    if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                                    {
                                        foreach (KeyValuePair<string, Tile> t in tiles)
                                        {
                                            if (t.Value._tileType == -1)
                                            {
                                                removedTiles.Add(tiles[t.Key].Position.ToPoint());
                                                addedTiles.Remove(tiles[t.Key].Position.ToPoint());
                                                tiles.Remove(t.Key);
                                            }
                                        }
                                    }

                                    tiles.Add(tileKeyName, new Tile(newTilePosition));

                                    if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                                    {
                                        tiles[tileKeyName]._tileType = 0;
                                    }
                                    else if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                                    {
                                        tiles[tileKeyName]._tileType = -1;
                                    }
                                    else if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                                    {
                                        tiles[tileKeyName]._tileType = -3;
                                    }

                                    addedTiles.Add(newTilePosition.ToPoint());
                                    editedTiles = true;

                                    if (newTilePosition.X / tileSize > arrayX)
                                    {
                                        arrayX = newTilePosition.X / tileSize;
                                    }
                                    if (Math.Abs(newTilePosition.Y) / tileSize > arrayY)
                                    {
                                        arrayY = Math.Abs(newTilePosition.Y) / tileSize;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (Mouse.GetState().RightButton == ButtonState.Pressed)
                {
                    Vector2 newTilePosition;
                    string tileKeyName;
                    for (int i = 0; i < drawSize; i++)
                    {
                        for (int j = 0; j < drawSize; j++)
                        {
                            newTilePosition = placeTilePosition() + new Vector2(i * tileSize, j * tileSize);
                            tileKeyName = $"{newTilePosition.X},{newTilePosition.Y}";

                            if (newTilePosition.X >= 0 && newTilePosition.Y <= 0)
                            {
                                if (tiles.ContainsKey(tileKeyName))
                                {
                                    removedTiles.Add(tiles[tileKeyName].Position.ToPoint());
                                    tiles.Remove(tileKeyName);
                                    editedTiles = true;
                                }
                            }
                        }
                    }
                }
                else if (editedTiles)
                {
                    ConfigureTileTypes(false);
                    editedTiles = false;
                }

                if (EditorManager.HaveIJustPressed(Keys.OemPlus) || EditorManager.HaveIJustPressed(Keys.Add))
                {
                    drawSize++;
                }
                else if (EditorManager.HaveIJustPressed(Keys.OemMinus) || EditorManager.HaveIJustPressed(Keys.Subtract))
                {
                    drawSize =  (drawSize > 1) ? drawSize - 1 : drawSize;
                }
            }
        }

        public static void LoadContent(ContentManager Content)
        {
            placeTileTexture = Content.Load<Texture2D>("placeTile");
            Tile.LoadTileTexture(Content, "UnVincedTilesV2");
        }

        public static void Draw(SpriteBatch SB)
        {
          
            SB.Draw(placeTileTexture, new Vector2(), null,Color.White, 0, Vector2.Zero, TileMapEditor.tileSize / 64.0f, SpriteEffects.None, 0f);

            foreach (Tile t in tiles.Values)
            {
                if (t != null) { t.Draw(SB); }
            }

            for (int i = 0; i < drawSize; i++)
            {
                for (int j = 0; j < drawSize; j++)
                {
                    SB.Draw(placeTileTexture, placeTilePosition() + new Vector2(i * tileSize, j * tileSize),  null, Color.White, 0, Vector2.Zero, TileMapEditor.tileSize / 64.0f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
