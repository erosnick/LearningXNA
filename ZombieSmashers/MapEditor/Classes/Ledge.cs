using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.Classes
{
    public class Ledge
    {
        public Vector2[] Nodes { get; set; }
        public int TotalNodes { get; set; }
        public int Flags { get; set; }

        public Ledge()
        {
             Nodes = new Vector2[16];
        }
    }
}
