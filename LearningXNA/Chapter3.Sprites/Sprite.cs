using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chapter3.Sprites
{
    class Sprite
    {
        public float Scale { get; set; } = 1.0f;

        public Texture2D Texture { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; } = new Vector2(0.0f, 0.0f);

        public float Width { get { return size.X * Scale; } }
        public float Height { get { return size.Y * Scale; } }

        Vector2 size;

        public Sprite(string textureName)
        {
            Texture = Global.Content.Load<Texture2D>(@textureName);
            this.size = new Vector2(Texture.Width, Texture.Height);
        }

        public Sprite(string textureName, Vector2 size)
        {
            Texture = Global.Content.Load<Texture2D>(@textureName);
            this.size = size;
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity;
        }

        public void EdgeDetect(ref Rectangle boundary)
        {
            if (Position.X < boundary.Left || Position.X > boundary.Right - Width)
            {
                Velocity = new Vector2(-Velocity.X, Velocity.Y);
            }

            if (Position.Y < boundary.Top || Position.Y > boundary.Bottom - Height)
            {
                Velocity = new Vector2(Velocity.X, -Velocity.Y);
            }
        }
    }
}
