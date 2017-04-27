using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shared
{
    public class Text
    {
        public float Size {get;set;} = 1.0f;
        public Color Color { get; set; } = Color.White;
        SpriteFont font;
        SpriteBatch sprite;
        private string text;

        public Text(SpriteBatch sprite, SpriteFont font)
        {
            this.sprite = sprite;
            this.font = font;
        }

        public void DrawText(int x, int y, string text)
        {
            this.text = text;

            sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            sprite.DrawString(font, text, new Vector2((float)x, (float)y), Color, 0.0f, new Vector2(), Size, SpriteEffects.None, 1.0f);

            sprite.End();
        }

        public bool DrawClickText(int x, int y, string text, int mouseX, int mouseY, bool mouseClick)
        {
            bool r = false;

            if (mouseX > x && mouseY > y &&
                mouseX < x + font.MeasureString(text).X * Size &&
                mouseY < y + font.MeasureString(text).Y * Size)
            {
                Color = Color.Yellow;
                if (mouseClick)
                {
                    r = true;
                }
            }

            DrawText(x, y, text);

            return r;
        }
        
        public Vector2 TextSize
        {
            get { return font.MeasureString(text); }
        }
    }
}
