using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MapEditor.Classes
{
    public class SegmentDefinition
    {
        public string Name { get; set; }
        public int SourceIndex { get; set; }
        public Rectangle SourceRect { get; set; }
        private int Flags { get; set; }

        public SegmentDefinition(string name, int sourceIndex, Rectangle sourceRect, int flags)
        {
            Name = name;
            SourceIndex = sourceIndex;
            SourceRect = sourceRect;
            Flags = flags;
        }
    }
}
