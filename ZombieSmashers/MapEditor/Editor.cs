using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MapEditor.Classes;
using Shared;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace MapEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Editor : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Text text;
        SpriteFont font;

        Map map;

        Texture2D[] mapsTexture;
        Texture2D nullTexture;

        Texture2D icons;
        int mouseX;
        int mouseY;
        bool rightMouseDown;
        bool leftButtonPressed;

        int mouseDragSegment = -1;
        int currentLayer = 1;
        int preMouseX;
        int preMouseY;

        bool middleMouseDown;
        Vector2 scroll;

        int paletteOffsetX = 600;

        List<Rectangle> tileSourceBounds;
        List<Rectangle> tileDestBounds;

        MouseState lastMouseState;
        MouseState currentMouseState;

        private string layerName = "";

        private delegate void EventHandler(MouseState mouseState);

        private event EventHandler OnClick;
        private event EventHandler OnPressed;
        private event EventHandler OnReleased;

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

            OnClick += OnMouseClick;
            OnPressed += OnMouseLeftButtonPressed;
            OnReleased += OnMouseLeftButtonReleased;

            leftButtonPressed = false;

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
            font = Content.Load<SpriteFont>(@"Fonts/Arial");
            text = new Text(spriteBatch, font);

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

        private void HandleInput()
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
            if (rightMouseDown)
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
            HandleInput();

            // TODO: Add your update logic here
            currentMouseState = Mouse.GetState();

            if (currentMouseState.LeftButton == ButtonState.Released &&
                lastMouseState.LeftButton == ButtonState.Pressed)
            {
                OnClick(currentMouseState);
                OnReleased(currentMouseState);
            }

            if (lastMouseState.LeftButton == ButtonState.Released &&
                currentMouseState.LeftButton == ButtonState.Pressed)
            {
                OnPressed(currentMouseState);
            }

            mouseX = currentMouseState.X;
            mouseY = currentMouseState.Y;

            if (leftButtonPressed)
            {
                if (!rightMouseDown && mouseX < paletteOffsetX)
                {
                    int index = map.GetHoveredSegment(mouseX, mouseY, currentLayer, scroll);

                    if (index != -1)
                    {
                        mouseDragSegment = index;
                    }
                }

                rightMouseDown = true;
            }
            else
            {
                rightMouseDown = false;
            }

            if (mouseDragSegment > -1)
            {
                if (!rightMouseDown)
                {
                    mouseDragSegment = -1;
                }
                else
                {
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

            AddSegment();

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

            DrawMapSegments();
            DrawCursor();
            DrawText();

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
            text.Size = 0.8f;

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

                text.Color = Color.White;
                text.DrawText(tileDestBounds[i].X + 50, tileDestBounds[i].Y, segmentDefinition.Name);
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
                    layerName = "background";
                    break;

                case 1:
                    layerName = "middle";
                    break;

                case 2:
                    layerName = "foreground";
                    break;
            }

            text.DrawClickText(5, 5, "Layer: " + layerName, mouseX, mouseY, false);
        }

        private void OnMouseLeftButtonPressed(MouseState mouseState)
        {
            leftButtonPressed = true;
            Console.WriteLine("OnMouseLeftButtonPressed");
        }

        private void OnMouseLeftButtonReleased(MouseState mouseState)
        {
            leftButtonPressed = false;
            Console.WriteLine("OnMouseLeftButtonReleased");
        }

        private void OnMouseClick(MouseState mouseState)
        {
            if (text.DrawClickText(5, 5, "Layer: " + layerName, mouseState.X, mouseState.Y, true))
            {
                currentLayer = (currentLayer + 1) % 3;
            }

            Console.WriteLine("OnClick");
        }
    }
}
