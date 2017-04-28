using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Shared
{
    public class Text
    {
        public static float Size {get;set;} = 1.0f;
        public static Color Color { get; set; } = Color.White;
        static SpriteFont font;
        static SpriteBatch sprite;
        static private string text;

        static Text()
        {
            var graphicDevice = GameServices.GetService<GraphicsDevice>();
            var content = GameServices.GetService<ContentManager>();
            sprite = new SpriteBatch(graphicDevice);
            font = content.Load<SpriteFont>(@"font/Arial");
        }

        public static void DrawText(int x, int y, string inText)
        {
            text = inText;

            sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            sprite.DrawString(font, text, new Vector2((float)x, (float)y), Color, 0.0f, new Vector2(), Size, SpriteEffects.None, 1.0f);

            sprite.End();
        }

        public static bool DrawClickText(int x, int y, string text, int mouseX, int mouseY, bool mouseClick)
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
