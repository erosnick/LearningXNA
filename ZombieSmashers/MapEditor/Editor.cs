﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MapEditor.Classes;
using Shared;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace MapEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Editor : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Map map;

        Texture2D[] mapsTexture;
        Texture2D nullTexture;

        Texture2D icons;
        int mouseX;
        int mouseY;
        bool canMoveTile;
        bool isDraggingSomething;
        bool leftButtonPressed;
        bool rightButtonPressed;

        int mouseDragSegment = -1;
        int currentLayer = 1;
        int preMouseX;
        int preMouseY;

        bool middleMouseDown;
        Vector2 scroll;

        int paletteOffsetX = 608;

        List<Rectangle> tileSourceBounds;
        List<Rectangle> tileDestBounds;

        MouseState lastMouseState;
        MouseState currentMouseState;

        private string layerName = "";

        int coolDown;

        Vector2 lastLocation;

        int currentLedge;
        int currentNode;

        bool mouseClick;

        List<Vector2> ledgePaletteLocation;

        KeyboardState oldKeyboardState;
        EditingMode editingMode = EditingMode.None;

        private delegate void EventHandler(MouseState mouseState);
        private delegate void EventHandlerNoParams();

        private event EventHandler onMouseLeftButtonClick;
        private event EventHandler onMouseRightButtonClick;
        private event EventHandler onMouseLeftButtonPressed;
        private event EventHandler onMouseLeftButtonReleased;
        private event EventHandler onMouseRightButtonPressed;
        private event EventHandler onMouseRightButtonReleased;
        private event EventHandler onMouseMove;
        private event EventHandlerNoParams OnCtrlPlusZ;
        private event EventHandlerNoParams OnCtrlPlusY;

        enum DrawingMode
        {
            SegmentSelection,
            CollisionMap,
            Ledges
        }

        enum EditingMode
        {
            None,
            Path
        }

        DrawingMode drawingMode = DrawingMode.SegmentSelection;

        public Editor()
        {

            AllocConsole();

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            Window.Position = new Point(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            onMouseLeftButtonClick += OnMouseLeftButtonClick;
            onMouseLeftButtonPressed += OnMouseLeftButtonPressed;
            onMouseLeftButtonReleased += OnMouseLeftButtonReleased;
            onMouseRightButtonClick += OnMouseRightButtonClick;
            onMouseRightButtonPressed += OnMouseRightButtonPressed;
            onMouseRightButtonReleased += OnMouseRightButtonReleased;
            onMouseMove += OnMouseMove;
            OnCtrlPlusZ += OnCtrlZ;
            OnCtrlPlusY += OnCtrlY;

            leftButtonPressed = false;

            GameServices.AddService<GraphicsDevice>(GraphicsDevice);
            GameServices.AddService<ContentManager>(Content);

            map = new Map();

            GenerateTileBounds();

            ledgePaletteLocation = new List<Vector2>();

            for (int i = 0; i < 16; i++)
            {
                var y = 50 + i * 20;
                ledgePaletteLocation.Add(new Vector2(paletteOffsetX, y));
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            nullTexture = Content.Load<Texture2D>(@"gfx/1x1");
            mapsTexture = new Texture2D[1];
            for (int i = 0; i < mapsTexture.Length; i++)
            {
                mapsTexture[i] = Content.Load<Texture2D>(@"gfx/maps" + (i + 1).ToString());
            }

            icons = Content.Load<Texture2D>(@"gfx/icons");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void HandleInput(int deltaTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                currentLayer = 0;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D2))
            {
                currentLayer = 1;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D3))
            {
                currentLayer = 2;
            }

            currentMouseState = Mouse.GetState();

            if (lastMouseState.LeftButton == ButtonState.Released &&
                currentMouseState.LeftButton == ButtonState.Pressed)
            {
                onMouseLeftButtonPressed(currentMouseState);
            }

            if (currentMouseState.LeftButton == ButtonState.Released &&
                lastMouseState.LeftButton == ButtonState.Pressed)
            {
                onMouseLeftButtonClick(currentMouseState);
                onMouseLeftButtonReleased(currentMouseState);
            }

            if (lastMouseState.X > 0 && lastMouseState.Y > 0)
            {
                if (currentMouseState.X != lastMouseState.X ||
                    currentMouseState.Y != lastMouseState.Y)
                {
                    onMouseMove(currentMouseState);
                }
            }

            if (lastMouseState.RightButton == ButtonState.Released &&
                currentMouseState.RightButton == ButtonState.Pressed)
            {
                onMouseRightButtonPressed(currentMouseState);
            }

            if (currentMouseState.RightButton == ButtonState.Released &&
                lastMouseState.RightButton == ButtonState.Pressed)
            {
                onMouseRightButtonClick(currentMouseState);
                onMouseRightButtonReleased(currentMouseState);
            }

            if (lastMouseState.X > 0 && lastMouseState.Y > 0)
            {
                if (currentMouseState.X != lastMouseState.X ||
                    currentMouseState.Y != lastMouseState.Y)
                {
                    onMouseMove(currentMouseState);
                }
            }

            var keys = Keyboard.GetState().GetPressedKeys();

            var hasCtrl = false;
            var hasZ = false;
            var hasY = false;

            foreach (var key in keys)
            {
                if (key == Keys.LeftControl)
                {
                    hasCtrl = true;
                }

                if (key == Keys.Z)
                {
                    hasZ = true;
                }

                if (key == Keys.Y)
                {
                    hasY = true;
                }
            }

            if (hasCtrl && hasZ)
            {
                if (coolDown <= 0.0f)
                {
                    OnCtrlPlusZ();
                    coolDown = 100;
                }
                else
                {
                    coolDown -= deltaTime;
                }
            }

            if (hasCtrl && hasY)
            {
                if (coolDown <= 0.0f)
                {
                    OnCtrlPlusY();
                    coolDown = 100;
                }
                else
                {
                    coolDown -= deltaTime;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Delete))
            {
                map.RemoveSegment(currentLayer);
            }

            UpdateKeys();
        }

        private void GenerateTileBounds()
        {
            tileDestBounds = new List<Rectangle>();
            tileSourceBounds = new List<Rectangle>();

            Rectangle sourceRect = new Rectangle();
            Rectangle destRect = new Rectangle();

            for (int i = 0; i < map.SegmentDefinitions.Count; i++)
            {
                SegmentDefinition segmentDefinition = map.SegmentDefinitions[i];

                destRect.X = paletteOffsetX;
                destRect.Y = 50 + i * 60;

                sourceRect = segmentDefinition.SourceRect;

                // Maintain scale never exceeding 45 pixels on either dimension
                if (sourceRect.Width > sourceRect.Height)
                {
                    destRect.Width = 45;
                    destRect.Height = (int)(((float)sourceRect.Height / (float)sourceRect.Width) * 45.0);
                }
                else
                {
                    destRect.Height = 45;
                    destRect.Width = (int)(((float)sourceRect.Width / (float)sourceRect.Height) * 45.0f);
                }

                tileSourceBounds.Add(sourceRect);
                tileDestBounds.Add(destRect);
            }
        }

        private void AddSegment()
        {
            for (int i = 0; i < tileSourceBounds.Count; i++)
            {
                if (mouseX > tileDestBounds[i].X && mouseX < 780 &&
                    mouseY > tileDestBounds[i].Y && mouseY < tileDestBounds[i].Y + 45)
                {
                    if (mouseDragSegment == -1)
                    {
                        int index = map.AddSegment(currentLayer, i);

                        if (index <= -1)
                        {
                            continue;
                        }

                        float layerScalar = 0.5f;

                        //if (currentLayer == 0)
                        //{
                        //    layerScalar = 0.375f;
                        //}
                        //else if (currentLayer == 2)
                        //{
                        //    layerScalar = 0.6125f;
                        //}

                        map.MapSegments[currentLayer][index].Location.X = (mouseX - tileSourceBounds[i].Width / 4 + scroll.X * layerScalar);
                        map.MapSegments[currentLayer][index].Location.Y = (mouseY - tileSourceBounds[i].Height / 4 + scroll.Y * layerScalar);
                        mouseDragSegment = index;

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            HandleInput(gameTime.ElapsedGameTime.Milliseconds);

            mouseX = currentMouseState.X;
            mouseY = currentMouseState.Y;

            if (mouseDragSegment > -1)
            {
                if (!canMoveTile)
                {
                    isDraggingSomething = false;
                    mouseDragSegment = -1;
                }
                else
                {
                    isDraggingSomething = true;
                    Vector2 location = map.MapSegments[currentLayer][mouseDragSegment].Location;

                    location.X += (mouseX - preMouseX);
                    location.Y += (mouseY - preMouseY);

                    map.MapSegments[currentLayer][mouseDragSegment].Location = location;
                }
            }

            middleMouseDown = (currentMouseState.MiddleButton == ButtonState.Pressed);

            if (middleMouseDown)
            {
                scroll.X -= (mouseX - preMouseX) * 2.0f;
                scroll.Y -= (mouseY - preMouseY) * 2.0f;
            }

            preMouseX = mouseX;
            preMouseY = mouseY;

            lastMouseState = currentMouseState;

            base.Update(gameTime);
        }

        private void UpdateKeys()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            Keys[] currentKeys = keyboardState.GetPressedKeys();
            Keys[] lastKeys = oldKeyboardState.GetPressedKeys();

            bool found = false;

            for (int i = 0; i < currentKeys.Length; i++)
            {
                found = false;

                for (int y = 0; y < lastKeys.Length; y++)
                {
                    if (currentKeys[i] == lastKeys[i])
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    PressKey(currentKeys[i]);
                }
            }

            oldKeyboardState = keyboardState;
        }

        private void PressKey(Keys key)
        {
            string temp = String.Empty;

            switch(editingMode)
            {
                case EditingMode.Path:
                    temp = map.Path;
                    break;

                default:
                    return;
            }

            if (key == Keys.Back)
            {
                if (temp.Length > 0)
                {
                    temp = temp.Substring(0, temp.Length - 1);
                }
            }
            else if (key == Keys.Enter)
            {
                editingMode = EditingMode.None;
            }
            else
            {
                temp = (temp + (char)key).ToLower();
            }

            switch (editingMode)
            {
                case EditingMode.Path:
                    map.Path = temp;
                    break;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            //text.Size = 1.0f;
            //text.Color = new Color(0, 0, 0, 125);

            //for (int i = 0; i < 3; i++)
            //{
            //    if (i == 2)
            //    {
            //        text.Color = Color.White;
            //    }

            //    text.DrawText(25 - i * 2, 250 - i * 2, "Zombie Smashers XNA FTW!");
            //}

            map.Draw(spriteBatch, mapsTexture, scroll);

            if (drawingMode == DrawingMode.SegmentSelection)
            {
                DrawMapSegments();
            }

            DrawGrid();

            if (drawingMode == DrawingMode.Ledges)
            {
                DrawLedges();
                DrawLedgePalette();
            }

            DrawButton(5, 65, 3, mouseX, mouseY, mouseClick);
            DrawButton(40, 65, 4, mouseX, mouseY, mouseClick);

            DrawCursor();
            DrawText();
            DrawInfo();
 
            base.Draw(gameTime);
        }

        private void DrawTransparentLayer(Rectangle destRect, Color color)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(nullTexture, destRect, color);

            spriteBatch.End();
        }

        private void DrawMapSegments()
        {
            DrawTransparentLayer(new Rectangle(paletteOffsetX, 20, 280, 550), new Color(0, 0, 0, 100));

            for (int i = 0; i < tileDestBounds.Count; i++)
            {
                SegmentDefinition segmentDefinition = map.SegmentDefinitions[i];

                if (segmentDefinition == null)
                {
                    continue;
                }

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                spriteBatch.Draw(mapsTexture[segmentDefinition.SourceIndex], tileDestBounds[i], tileSourceBounds[i], Color.White);

                spriteBatch.End();

                Text.Color = Color.White;
                Text.DrawText(tileDestBounds[i].X + 50, tileDestBounds[i].Y, segmentDefinition.Name);
            }
        }

        private void DrawCursor()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(icons, new Vector2(mouseX, mouseY), new Rectangle(0, 0, 32, 32), Color.White);

            spriteBatch.End();
        }

        private void DrawText()
        {
            switch (currentLayer)
            {
                case 0:
                    layerName = "0:background";
                    break;

                case 1:
                    layerName = "1:middle";
                    break;

                case 2:
                    layerName = "2:foreground";
                    break;
            }

            Text.DrawClickText(5, 5, "Layer: " + layerName, mouseX, mouseY, false);

            switch (drawingMode)
            {
                case DrawingMode.SegmentSelection:
                    layerName = "segement";
                    break;

                case DrawingMode.CollisionMap:
                    layerName = "collison";
                    break;

                case DrawingMode.Ledges:
                    layerName = "ledge";
                    break;
            }

            Text.DrawClickText(5, 25, "draw: " + layerName, mouseX, mouseY, false);

            Text.Color = Color.White;
            if (editingMode == EditingMode.Path)
            {
                Text.DrawText(5, 45, map.Path + "*");
            }
            else
            {
                Text.DrawClickText(5, 45, map.Path, mouseX, mouseY, false);
            }
        }

        private void DrawInfo()
        {
            var mouseState = Mouse.GetState();
            
            Text.DrawText(150, 5, String.Format("X = {0}, Y = {1}", mouseState.X, mouseState.Y));
        }

        private void DrawGrid()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    Rectangle destRect = new Rectangle(x * 32 - (int)(scroll.X / 2),
                                                       y * 32 - (int)(scroll.Y / 2),
                                                       32, 32);

                    if (x < 19)
                    {
                        spriteBatch.Draw(nullTexture, new Rectangle(destRect.X, destRect.Y, 32, 1), new Color(255, 0, 0, 100));
                    }

                    if (y < 19)
                    {
                        spriteBatch.Draw(nullTexture, new Rectangle(destRect.X, destRect.Y, 1, 32), new Color(255, 0, 0, 100));
                    }

                    if (x < 19 && y < 19)
                    {
                        if (map.Grid[x, y] == 1)
                        {
                            spriteBatch.Draw(nullTexture, destRect, new Color(255, 0, 0, 100));
                        }
                    }
                }
            }

            var color = new Color(255, 255, 255, 100);
            spriteBatch.Draw(nullTexture, new Rectangle(100, 50, 400, 1), color);
            spriteBatch.Draw(nullTexture, new Rectangle(100, 50, 1, 500), color);
            spriteBatch.Draw(nullTexture, new Rectangle(500, 50, 1, 500), color);
            spriteBatch.Draw(nullTexture, new Rectangle(100, 550, 400, 1), color);

            spriteBatch.End();
        }

        private void DrawLedges()
        {
            Rectangle rect = new Rectangle();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Color tColor = new Color();

            rect.X = 32;
            rect.Y = 0;
            rect.Width = 32;
            rect.Height = 32;

            for (int i = 0; i < 16; i++)
            {
                for (int n = 0; n < map.Ledges[i].TotalNodes; n++)
                {
                    if (map.Ledges[i] != null && map.Ledges[i].TotalNodes > 0)
                    {
                        var tVect = map.Ledges[i].Nodes[n];
                        tVect -= scroll / 2.0f;
                        tVect.X -= 5.0f;

                        if (currentLedge == i)
                        {
                            tColor = Color.Yellow;
                        }
                        else
                        {
                            tColor = Color.White;
                        }

                        spriteBatch.Draw(icons, tVect, rect, tColor, 0.0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0.0f);

                        if (n < map.Ledges[i].TotalNodes - 1)
                        {
                            var nVect = map.Ledges[i].Nodes[n + 1];
                            nVect -= scroll / 2.0f;
                            nVect.X -= 4.0f;

                            for (int x = 1; x < 20; x++)
                            {
                                var iVect = tVect + (nVect - tVect) * ((float)x / 20.0f);

                                var nColor = new Color(255, 255, 255, 75);

                                if (map.Ledges[i].Flags == 1)
                                {
                                    nColor = new Color(255, 0, 0, 75);

                                    spriteBatch.Draw(icons, iVect, rect, nColor, 0.0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0.0f);
                                }
                            }
                        }
                    }
                }
            }

            spriteBatch.End();
        }

        private void DrawLedgePalette()
        {
            for (int i = 0; i < 16; i++)
            {
                if (map.Ledges[i] == null)
                {
                    continue;
                }

                var y = (int)ledgePaletteLocation[i].Y;

                if (currentLedge == i)
                {
                    Text.Color = Color.Lime;
                    Text.DrawText(paletteOffsetX, y, "ledge " + i.ToString());
                }
                else
                {
                    Text.DrawClickText(paletteOffsetX, y, "ledge " + i.ToString(), mouseX, mouseY, mouseClick);
                }

                Text.Color = Color.White;
                Text.DrawText(708, y, "n" + map.Ledges[i].TotalNodes.ToString());

                Text.DrawClickText(768, y, "f" + map.Ledges[i].Flags.ToString(), mouseX, mouseY, mouseClick);
            }
        }

        private bool DrawButton(int x, int y, int index, int mouseX, int mouseY, bool mouseClick)
        {
            var r = false;

            var sourceRect = new Rectangle(32 * (index % 8), 32 * (index / 8), 32, 32);
            var destRect = new Rectangle(x, y, 32, 32);

            if (destRect.Contains(mouseX, mouseY))
            {
                destRect.X -= 1;
                destRect.Y -= 1;
                destRect.Width += 2;
                destRect.Height += 2;

                if (mouseClick)
                {
                    r = true;
                }
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(icons, destRect, sourceRect, Color.White);

            spriteBatch.End();

            return r;
        }

        private bool GetCanEdit()
        {
            if (mouseX > 100 && mouseX < paletteOffsetX && mouseY > 100 && mouseY < 550)
            {
                return true;
            }

            return false;
        }

        private void OnMouseLeftButtonPressed(MouseState mouseState)
        {
            leftButtonPressed = true;

            if (drawingMode == DrawingMode.SegmentSelection)
            {
                AddSegment();
            }

            if (!canMoveTile)
            {
                if (GetCanEdit())
                {
                    if (drawingMode == DrawingMode.SegmentSelection)
                    {
                        if (!canMoveTile && mouseState.Y < paletteOffsetX)
                        {
                            int index = map.GetHoveredSegment(mouseState.X, mouseState.Y, currentLayer, scroll);

                            if (index != -1)
                            {
                                mouseDragSegment = index;
                            }
                        }

                        canMoveTile = true;
                    }
                    else if (drawingMode == DrawingMode.CollisionMap)
                    {
                        int x = (mouseState.X + (int)(scroll.X / 2)) / 32;
                        int y = (mouseState.Y + (int)(scroll.Y / 2)) / 32;

                        if (x >= 0 && x < map.GridSizeX && y >= 0 && y < map.GridSizeY)
                        {
                            map.Grid[x, y] = 1;
                        }
                    }
                    else if (drawingMode == DrawingMode.Ledges)
                    {
                        if (map.Ledges[currentLedge] == null)
                        {
                            map.Ledges[currentLedge] = new Ledge();
                        }

                        if (map.Ledges[currentLedge].TotalNodes < 15)
                        {
                            map.Ledges[currentLedge].Nodes[map.Ledges[currentLedge].TotalNodes] = new Vector2(mouseX, mouseY) + scroll / 2.0F;

                            map.Ledges[currentLedge].TotalNodes++;
                        }
                    }
                }

                canMoveTile = true;
            }

            if (mouseDragSegment >= 0)
            {
                lastLocation = map.MapSegments[currentLayer][mouseDragSegment].Location;
                Console.WriteLine(String.Format("lastLocation:{0}, {1}", lastLocation.X, lastLocation.Y));
            }

            Console.WriteLine("OnMouseLeftButtonPressed");
        }

        private void OnMouseLeftButtonClick(MouseState mouseState)
        {
            mouseClick = true;

            if (Text.DrawClickText(5, 5, "Layer: " + layerName, mouseState.X, mouseState.Y, mouseClick))
            {
                currentLayer = (currentLayer + 1) % 3;
            }

            if (Text.DrawClickText(5, 25, "draw: " + layerName, mouseX, mouseY, mouseClick))
            {
                drawingMode = (DrawingMode)((int)(drawingMode + 1) % 3);
            }

            if (drawingMode == DrawingMode.Ledges)
            {
                for (int i = 0; i < 16; i++)
                {
                    var y = (int)ledgePaletteLocation[i].Y;

                    if (Text.DrawClickText(paletteOffsetX, y, "ledge " + i.ToString(), mouseX, mouseY, mouseClick))
                    {
                        currentLedge = i;
                    }

                    if (Text.DrawClickText(768, y, "f" + map.Ledges[i].Flags.ToString(), mouseX, mouseY, mouseClick))
                    {
                        map.Ledges[i].Flags = (map.Ledges[i].Flags + 1) % 2;
                    }
                }
            }

            if (Text.DrawClickText(5, 45, map.Path, mouseX, mouseY, mouseClick))
            {
                editingMode = EditingMode.Path;
            }

            if (DrawButton(5, 65, 3, mouseX, mouseY, mouseClick))
            {
                map.Save();
            }

            if (DrawButton(40, 65, 4, mouseX, mouseY, mouseClick))
            {
                map.Load();
            }

            Console.WriteLine("X = {0}, Y = {1}", mouseState.X, mouseState.Y);
        }

        private void OnMouseLeftButtonReleased(MouseState mouseState)
        {
            mouseClick = false;

            if (isDraggingSomething)
            {
                var command = new MoveCommand(map.MapSegments[currentLayer][mouseDragSegment], lastLocation);

                CommandManager.ExecuteCommand(command);

                Console.WriteLine("Drag");
            }

            leftButtonPressed = false;
            canMoveTile = false;

            Console.WriteLine("OnMouseLeftButtonReleased");
        }

        private void OnMouseRightButtonPressed(MouseState mouseState)
        {
            rightButtonPressed = true;

            if (drawingMode == DrawingMode.CollisionMap)
            {
                int x = (mouseState.X + (int)(scroll.X / 2)) / 32;
                int y = (mouseState.Y + (int)(scroll.Y / 2)) / 32;

                if (x >= 0 && x < map.GridSizeX && y >= 0 && y < map.GridSizeY)
                {
                    map.Grid[x, y] = 0;
                }
            }

            Console.WriteLine("OnMouseRightButtonPressed");
        }

        private void OnMouseRightButtonClick(MouseState mouseState)
        {
            Console.WriteLine("OnMouseRightButtonClick");
        }

        private void OnMouseRightButtonReleased(MouseState mouseState)
        {
            rightButtonPressed = false;

            Console.WriteLine("OnMouseRightButtonReleased");
        }

        private void OnMouseMove(MouseState mouseState)
        {
        }

        private void OnCtrlZ()
        {
            CommandManager.UndoCommand();
            Console.WriteLine("Undo");
        }

        private void OnCtrlY()
        {
            CommandManager.RedoCommand();
            Console.WriteLine("Redo");
        }
    }
}
