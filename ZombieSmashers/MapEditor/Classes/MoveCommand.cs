using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MapEditor.Classes
{
    class MoveCommand : ICommand
    {
        private Vector2 oldLocation;
        private Vector2 newLocation;
        private MapSegment segment;

        public MoveCommand(MapSegment segment, Vector2 newLocation)
        {
            this.segment = segment;
            oldLocation = newLocation;
            this.newLocation = segment.Location;
        }

        public void Execute()
        {
            segment.Location = newLocation;
        }

        public void UndoExecute()
        {
            segment.Location = oldLocation;
        }
    }
}
