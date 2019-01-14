using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chapter3.Sprites
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texture;
        Texture2D textureTransparent;
        Sprite dumpling;
        Sprite threerings;
        Point frameSize;
        Point currentFrame;
        Point sheetSize;
        Rectangle boundary;

        float timeSinceLastFrame;
        float animationRate;
        float millisecondsPerFrame;

        public int WindowWidth
        {
            get { return Window.ClientBounds.Width; }
        }

        public int WindowHeight
        {
            get { return Window.ClientBounds.Height; }
        }

        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;

            Content.RootDirectory = "Content";

            Global.Graphics = graphics;
            Global.Content = Content;

            frameSize = new Point(75, 75);
            currentFrame = new Point(0, 0);
            sheetSize = new Point(6, 8);

            timeSinceLastFrame = 0.0f;
            animationRate = 30.0f;

            boundary = new Rectangle(0, 0, 1600, 900);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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
            texture = Content.Load<Texture2D>(@"images/logo");
            textureTransparent = Content.Load<Texture2D>(@"images/logo_trans");

            dumpling = new Sprite(@"images/Dumpling");

            dumpling.Scale = 0.5f;
            dumpling.Position = new Vector2(0, WindowHeight / 2 - dumpling.Height / 2);
            dumpling.Velocity = new Vector2(5.0f, 3.0f);

            threerings = new Sprite(@"images/threerings", new Vector2(frameSize.X, frameSize.Y));
            threerings.Velocity = new Vector2(3.0f, 6.0f);
   
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            dumpling.EdgeDetect(ref boundary);
            dumpling.Update(gameTime);

            threerings.EdgeDetect(ref boundary);
            threerings.Update(gameTime);

            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;

            millisecondsPerFrame = 1000.0f / animationRate;

            if (timeSinceLastFrame >= millisecondsPerFrame)
            {
                currentFrame.X++;

                if (currentFrame.X >= sheetSize.X)
                {
                    currentFrame.X = 0;

                    currentFrame.Y++;

                    if (currentFrame.Y >= sheetSize.Y)
                    {
                        currentFrame.Y = 0;
                    }
                }

                timeSinceLastFrame = 0.0f;
            }

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
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            spriteBatch.Draw(texture, new Vector2(WindowWidth / 2 - texture.Width / 2, 0.0f), Color.White);

            float positionX = WindowWidth / 2 - textureTransparent.Width / 2;
            float positionY = WindowHeight / 2 - textureTransparent.Height / 2;

            spriteBatch.Draw(textureTransparent, new Vector2(positionX, positionY), null, Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 1.0f);

            spriteBatch.Draw(dumpling.Texture, dumpling.Position, null, Color.White, 0.0f, new Vector2(0.0f, 0.0f), dumpling.Scale, SpriteEffects.None, 0.0f);

            spriteBatch.Draw(threerings.Texture, threerings.Position, new Rectangle(currentFrame.X * frameSize.X, currentFrame.Y * frameSize.Y, frameSize.X, frameSize.Y), Color.White, 0.0f, new Vector2(0.0f, 0.0f), threerings.Scale, SpriteEffects.None, 0.0f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
