using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ZombieSmashers
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameScene : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public GameScene()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
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

            base.Update(gameTime);
        }
        public Texture2D CreateCircle(int radius)
        {
            int outerRadius = radius * 2 + 2; // So circle doesn't go out of bounds
            Texture2D texture = new Texture2D(GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            // Colour the entire texture transparent first.
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.TransparentBlack;

            // Work out the minimum step necessary using trigonometry + sine approximation.
            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)
            {
                // Use the parametric definition of a circle: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * outerRadius + x + 1] = Color.White;
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            //Texture2D circle = CreateCircle(100);

            //BasicEffect basicEffect = new BasicEffect(GraphicsDevice);
            //basicEffect.Texture = null;
            //basicEffect.TextureEnabled = true;

            //VertexPositionTexture[] vert = new VertexPositionTexture[4];
            //vert[0].Position = new Vector3(0, 0, 0);
            //vert[1].Position = new Vector3(100, 0, 0);
            //vert[2].Position = new Vector3(0, 100, 0);
            //vert[3].Position = new Vector3(100, 100, 0);

            //vert[0].TextureCoordinate = new Vector2(0, 0);
            //vert[1].TextureCoordinate = new Vector2(1, 0);
            //vert[2].TextureCoordinate = new Vector2(0, 1);
            //vert[3].TextureCoordinate = new Vector2(1, 1);

            //short[] ind = new short[6];
            //ind[0] = 0;
            //ind[1] = 2;
            //ind[2] = 1;
            //ind[3] = 1;
            //ind[4] = 2;
            //ind[5] = 3;

            //foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            //{

            //    effectPass.Apply();
            //    GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vert, 0, vert.Length, ind, 0, ind.Length / 3);

            //}


            base.Draw(gameTime);
        }
    }
}
