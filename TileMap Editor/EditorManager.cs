using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace TileMap_Editor
{
    public static class EditorManager
    {
        public static string state = "Paused";

        static Texture2D overlayTexture;
        static SpriteFont menuFont;
        static Keys lastKeyDown;
        static Color overlayColor = new Color(Color.Black, 0.2f);

        static string[] MapsToLoad;
        static string[] menuOptions = {"New Tile Map", "Load Tile Map", "Save Tile Map"};
        static string[] editorOptions = { "General", "Loading", "Saving" };
        static string lastKeyPushed = "";
        static string fileString = "", tileMapFileExt = ".tilemap", tileMapsFileFolder = "\\Maps";
        static int currentOption = 0, editorOption = 0;
        static float menuY = -Game1.screenHeight + TileMapEditor.tileSize - (Camera.Position.Y - (Game1.screenHeight - TileMapEditor.tileSize));
        static float menuX = -Camera.Position.X;
        static float cameraSpeedTextAlpha = 0;

        static Dictionary<string, char> allowedKeys = new Dictionary<string, char>()
        {
            {"Space" , '_' }, { "OemMinus" , '-'},
        };

        private static List<Notice> Notices = new();
        private static List<Notice> NoticesToRemove = new();

        class Notice
        {
            public Color color;

            public string text;
            public float alpha;
            Vector2 position = new();

            public Notice(string Text = "", float Alpha = 0)
            {
                text = Text;
                alpha = Alpha;
                color = new Color(Color.Red, Alpha);
            }
            
            public void DecrementAlpha(float amt = 0.01f)
            {
                alpha -= amt;
                Debug.WriteLine(alpha);
                color = new Color(Color.Red, alpha);
            }

            public Vector2 GetPosition(int noticeLevel = 0)
            {
                position.X = XPos();
                position.Y = YPos(noticeLevel);

                return position;
            }

            private float XPos()
            {
                return menuX + (0.99f * Game1.screenWidth - menuFont.MeasureString(text).X) - Camera.Position.X;
            }

            private float YPos(int level = 0)
            {
                return menuY + (Game1.screenHeight * 0.01f) - Camera.Position.Y + Camera.bottomLeftTile + (level * menuFont.MeasureString(text).Y);
            }
        }

        public static void LoadContent(ContentManager Content)
        {
            Content.RootDirectory = "Content";
            overlayTexture = Content.Load<Texture2D>("Base");
            menuFont = Content.Load<SpriteFont>("MenuOption");
        }

        static string[] GetSavedMaps()
        {
            string[] files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}{tileMapsFileFolder}");

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Split("\\")[files[i].Split("\\").Length - 1];
                files[i] = files[i].Split(tileMapFileExt)[0];
            }

            return files;
        }

        static void Do(int menuOpt)
        {
            switch (menuOptions[menuOpt])
            {
                case "Save Tile Map":
                    editorOption = 2;
                    fileString = string.Empty;
                    break;
                case "New Tile Map":
                    editorOption = 0;
                    state = "Editing";
                    TileMapEditor.tiles.Clear();

                    TileMapEditor.tileSize = TileMapEditor.baseTileSize;
                    Camera.bottomLeftTile = (Game1.screenHeight - TileMapEditor.tileSize);
                    Camera.Position = new(0, (Game1.screenHeight - TileMapEditor.tileSize), 0);


                    break;
                case "Load Tile Map":
                    editorOption = 1;
                    MapsToLoad = GetSavedMaps();
                    currentOption = 0;
                    break;
            }
        }

        static Vector2 CameraSpeedText()
        {
            return new Vector2(menuX + (0.99f * Game1.screenWidth - menuFont.MeasureString($"Camera Speed: {Camera.moveSpeed}").X) - Camera.Position.X, menuY + Game1.screenHeight * 0.01f - Camera.Position.Y + Camera.bottomLeftTile);
        }

        static void LoadMap(string file)
        {
            TileMapEditor.tiles.Clear();

            int fileMaxLines = File.ReadAllBytes(file).Count(s => s == 15);
            using (var stream = File.Open(file, FileMode.Open))
            {
                TileMapEditor.arrayY = fileMaxLines;

                using (BinaryReader br = new BinaryReader(stream))
                {
                    try
                    {
                        int i = 0;
                        int lineNumber = 1;
                        while (true)
                        {
                            switch ((int)br.ReadByte())
                            {
                                case 1:
                                    TileMapEditor.tiles.Add($"{i * TileMapEditor.tileSize},{lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize}", new Tile(new Vector2(i * TileMapEditor.tileSize, lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize)));
                                    TileMapEditor.tiles[$"{i * TileMapEditor.tileSize},{lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize}"]._tileType = 0;
                                    i++;
                                    break;
                                case 2:
                                    TileMapEditor.tiles.Add($"{i * TileMapEditor.tileSize},{lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize}", new Tile(new Vector2(i * TileMapEditor.tileSize, lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize)));
                                    i++;
                                    break;
                                case 3:
                                    TileMapEditor.tiles.Add($"{i * TileMapEditor.tileSize},{lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize}", new Tile(new Vector2(i * TileMapEditor.tileSize, lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize)));
                                    TileMapEditor.tiles[$"{i * TileMapEditor.tileSize},{lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize}"]._tileType = -1;
                                    i++;
                                    break;
                                case 4:
                                    TileMapEditor.tiles.Add($"{i * TileMapEditor.tileSize},{lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize}", new Tile(new Vector2(i * TileMapEditor.tileSize, lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize)));
                                    TileMapEditor.tiles[$"{i * TileMapEditor.tileSize},{lineNumber * TileMapEditor.tileSize - fileMaxLines * TileMapEditor.tileSize}"]._tileType = -3;
                                    i++;
                                    break;
                                case 15:
                                    lineNumber++;
                                    TileMapEditor.arrayX = i;
                                    i = 0;
                                    break;
                                default:
                                    i++;
                                    break;
                            }
                        }
                    }
                    catch (EndOfStreamException e){}

                    TileMapEditor.ConfigureTileTypes();
                }
            }
        }

        static void ReloadMap(Tile[,] mapArray)
        {
            TileMapEditor.tiles.Clear();

            for (int i = 0; i < mapArray.GetLength(0); i++)
            {
                for (int j = 0; j < mapArray.GetLength(1); j++)
                {
                    switch (mapArray[i, j])
                    {
                        case null:
                            break;
                        default:
                            TileMapEditor.tiles.Add($"{i * TileMapEditor.tileSize},{-j * TileMapEditor.tileSize}", mapArray[i, j]);
                            TileMapEditor.tiles[$"{i * TileMapEditor.tileSize},{-j * TileMapEditor.tileSize}"].Position = new Vector2(i * TileMapEditor.tileSize, -j * TileMapEditor.tileSize);
                            break;
                    }
                }
            }
        }

        public static void Update()
        {
            switch (editorOptions[editorOption]) 
            {
                case "General":
                    if (HaveIJustPressed(Keys.Escape))
                    {
                        //pauseButtonJustPressed = true;
                        state = (state == "Paused") ? "Editing" : "Paused";
                        if (state == "Paused")
                        {
                            menuY = -Game1.screenHeight + TileMapEditor.tileSize - ((int)Camera.Position.Y - (Game1.screenHeight - TileMapEditor.tileSize));
                            menuX = -Camera.Position.X;
                            currentOption = 0;
                        }
                    }

                    if (state == "Paused")
                    {
                        if (HaveIJustPressed(Keys.Up))
                        {
                            currentOption = (currentOption == 0) ? menuOptions.Length - 1 : currentOption - 1;
                        }
                        else if (HaveIJustPressed(Keys.Down))
                        {
                                currentOption = (currentOption == menuOptions.Length - 1) ? 0 : currentOption + 1;
                        }

                        if (HaveIJustPressed(Keys.Enter))
                        {
                            Do(currentOption);
                        }
                    }
                    else
                    {
                        if (HaveIJustPressed(Keys.I))
                        {
                            Tile[,] TileMap = TileMapEditor.ToTileArray();
                            TileMapEditor.tileSize = (TileMapEditor.tileSize < 128) ? TileMapEditor.tileSize+4 : 128;
                            
                            Camera.bottomLeftTile = (Game1.screenHeight - TileMapEditor.tileSize);
                            menuY = -Game1.screenHeight + TileMapEditor.tileSize - (Camera.Position.Y - (Game1.screenHeight - TileMapEditor.tileSize));
                            menuX = -Camera.Position.X;
                            ReloadMap(TileMap);

                        }
                        else if (HaveIJustPressed(Keys.O))
                        {
                            Tile[,] TileMap = TileMapEditor.ToTileArray();
                            TileMapEditor.tileSize = (TileMapEditor.tileSize > 4) ? TileMapEditor.tileSize - 4 : 4;
                            Camera.bottomLeftTile = (Game1.screenHeight - TileMapEditor.tileSize);
                            menuY = -Game1.screenHeight + TileMapEditor.tileSize - (Camera.Position.Y - (Game1.screenHeight - TileMapEditor.tileSize));
                            menuX = -Camera.Position.X;
                            ReloadMap(TileMap);
                        }

                        for (int k = 48; k < 58; k++)
                        {
                            if (HaveIJustPressed((Keys)k))
                            {
                                Camera.moveSpeed = 5 * (k - 47);

                                Notices.Add(new Notice($"Camera Speed: {Camera.moveSpeed}", 1));

                                //cameraSpeedTextAlpha = 1;
                            }
                        }
                        
                    }
                break;

                case "Saving":
                    if (HaveIJustPressed(Keys.Escape))
                    {
                        editorOption = 0;
                    }
                    else if (HaveIJustPressed(Keys.Back))
                    {
                        fileString = (fileString.Length > 0) ? fileString.Substring(0, fileString.Length - 1) : "";
                    }
                    else if (HaveIJustPressed(Keys.Enter))
                    {
                        if (!Directory.Exists($"{Directory.GetCurrentDirectory()}{tileMapsFileFolder}"))
                        {
                            Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}{tileMapsFileFolder}");
                        }

                        using (BinaryWriter sw = new BinaryWriter(File.Create($"{Directory.GetCurrentDirectory()}{tileMapsFileFolder}\\{fileString}.tilemap")))
                        {
                            int[,] tilePositions = TileMapEditor.ToArray();

                            for (int j = tilePositions.GetLength(1) - 1; j > -1; j--)
                            {
                                for (int i = 0; i < tilePositions.GetLength(0); i++)
                                {
                                    sw.Write((byte)tilePositions[i, j]);
                                }
                                sw.Write((byte)15);
                            }

                           
                        }
                        if (fileString != string.Empty)
                        {
                            editorOption = 0;
                            state = "Editing";
                        }
                    }
                    else
                    {
                        //AMELIORATE
                        

                        lastKeyPushed = (Keyboard.GetState().GetPressedKeys().Length == 0) ? "" : lastKeyPushed;

                        Keys[] keysPressed = Keyboard.GetState().GetPressedKeys();

                        foreach (Keys k in keysPressed) 
                        {
                            if (allowedKeys.ContainsKey(k.ToString()) && HaveIJustPressed(k))
                            {
                                fileString += allowedKeys[k.ToString()];
                            }
                            else
                            {
                                int code = (int)k.ToString()[0];
                                if (code > 64 && code < 127 && lastKeyPushed != k.ToString() && k.ToString().Length < 2)
                                {
                                    if (code <= 90)
                                    {
                                        if (keysPressed.Contains(Keys.LeftShift) || Keyboard.GetState().CapsLock)
                                        {
                                            fileString += k.ToString();
                                        }
                                        else
                                        {
                                            fileString += k.ToString().ToLower();
                                        }
                                    }
                                }
                            }

                            lastKeyPushed = k.ToString();
                            break;
                        }
                    }
                    break;

                case "Loading":
                    if (HaveIJustPressed(Keys.Up))
                    {
                        currentOption = (currentOption == 0) ? MapsToLoad.Length - 1 : currentOption - 1;
                    }
                    else if (HaveIJustPressed(Keys.Down))
                    {
                        currentOption = (currentOption == MapsToLoad.Length - 1) ? 0 : currentOption + 1;
                    }
                    else if (HaveIJustPressed(Keys.Enter))
                    {
                        TileMapEditor.tileSize = TileMapEditor.baseTileSize;
                        Camera.bottomLeftTile = (Game1.screenHeight - TileMapEditor.tileSize);
                        Camera.Position = new(0, (Game1.screenHeight - TileMapEditor.tileSize), 0);
                        LoadMap($"{Directory.GetCurrentDirectory()}{tileMapsFileFolder}\\{MapsToLoad[currentOption]}{tileMapFileExt}");
                        editorOption = 0;
                        state = "Editing";
                        
                    }
                    else if (HaveIJustPressed(Keys.Back)) 
                    {
                        editorOption = 0;
                        state = "Paused";
                    }
                    break;
            }

            LastKeyDown();
        }

        public static void Draw(SpriteBatch SB)
        {
            if (state == "Paused")
            {
                SB.Draw(overlayTexture, new Rectangle((int)menuX, (int)menuY, Game1.screenWidth, Game1.screenHeight), overlayColor);
            }
            switch (editorOptions[editorOption])
            {
                case "General":
                    if (state == "Paused")
                    {
                        int count = 1;
                        foreach (string opt in menuOptions)
                        {
                            SB.DrawString(menuFont, opt,
                                new Vector2(15 - Camera.Position.X, menuY + 15 * count + (count - 1) * menuFont.MeasureString(opt).Y)
                                , (menuOptions[currentOption] == opt) ? Color.Red : Color.White);
                            count++;
                        }
                    }

                    for (int count = 0; count < Notices.Count; count++)
                    {
                        if (Notices[count].alpha < 0)
                        {
                            NoticesToRemove.Add(Notices[count]);
                        }
                        else
                        {
                            SB.DrawString(menuFont, Notices[count].text, Notices[count].GetPosition(count), Notices[count].color);
                            Notices[count].DecrementAlpha();
                        }
                    }
                    foreach (Notice N in NoticesToRemove)
                    {
                        Notices.Remove(N);
                    }
                    /*else if (cameraSpeedTextAlpha > 0)
                    {
                        
                        SB.DrawString(menuFont, $"Camera Speed: {Camera.moveSpeed}", CameraSpeedText(), new Color(Color.Red, cameraSpeedTextAlpha));
                        cameraSpeedTextAlpha -= 0.01f;
                    }*/
                    break;

                case "Saving":

                    SB.Draw(overlayTexture,
                        new Rectangle((int)menuX + (int)(Game1.screenWidth * 0.05), (int)(Game1.screenHeight * 0.4 + menuY), (int)(Game1.screenWidth * 0.9), (int)(Game1.screenHeight * 0.1)),
                        Color.Black);

                    SB.Draw(overlayTexture, 
                        new Rectangle((int)menuX + (int)(Game1.screenWidth * 0.06), (int)(Game1.screenHeight * 0.41 + menuY), (int)(Game1.screenWidth*0.88), (int)(Game1.screenHeight * 0.08)),
                        Color.White);

                    SB.DrawString(menuFont, fileString, new Vector2((int)menuX + (int)(Game1.screenWidth * 0.07), (int)(Game1.screenHeight * 0.42 + menuY)), Color.Black);

                    break;

                case "Loading":
                    int loadCount = 1;
                    foreach (string opt in MapsToLoad)
                    {
                        SB.DrawString(menuFont, opt,
                            new Vector2(15 - Camera.Position.X, menuY + 15 * loadCount + (loadCount - 1) * menuFont.MeasureString(opt).Y)
                            , (MapsToLoad[currentOption] == opt) ? Color.Red : Color.White);
                        loadCount++;
                    }
                    break;
            }
        }
        
        public static void LastKeyDown()
        {
            if (Keyboard.GetState().GetPressedKeyCount() == 0)
            {
                lastKeyDown = Keys.None;
            }
        }

        public static bool HaveIJustPressed(Keys keyToCheck)
        {
            if (Keyboard.GetState().IsKeyDown(keyToCheck) && lastKeyDown != keyToCheck)
            {
                lastKeyDown = keyToCheck;
                return true;
            }
            return false;
        }
    }
}
