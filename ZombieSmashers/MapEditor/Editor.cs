using Microsoft.Xna.Framework;
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
            CollisionMap
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
            map = new Map();

            tileDestBounds = new List<Rectangle>();
            tileSourceBounds = new List<Rectangle>();

            GenerateTileBounds();

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
        }

        private void GenerateTileBounds()
        {
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
            if (leftButtonPressed)
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

            //AddSegment();

            lastMouseState = currentMouseState;

            base.Update(gameTime);
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
            }

            Text.DrawClickText(5, 25, "draw: " + layerName, mouseX, mouseY, false);
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

        private bool GetCanEdit()
        {
            if (mouseX > 100 && mouseY < paletteOffsetX && mouseY > 100 && mouseY < 550)
            {
                return true;
            }

            return false;
        }

        private void OnMouseLeftButtonPressed(MouseState mouseState)
        {
            leftButtonPressed = true;

            AddSegment();

            if (!canMoveTile)
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
            if (Text.DrawClickText(5, 5, "Layer: " + layerName, mouseState.X, mouseState.Y, true))
            {
                currentLayer = (currentLayer + 1) % 3;
            }

            if (Text.DrawClickText(5, 25, "draw: " + layerName, mouseX, mouseY, true))
            {
                drawingMode = (DrawingMode)((int)(drawingMode + 1) % 2);
            }

            Console.WriteLine("X = {0}, Y = {1}", mouseState.X, mouseState.Y);
        }

        private void OnMouseLeftButtonReleased(MouseState mouseState)
        {
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
