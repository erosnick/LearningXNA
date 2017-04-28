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
        private Vector2 oldPosition;

        public void Execute(MapSegment segment, Vector2 newLocation)
        {
            oldPosition = segment.Location;
            segment.Location = newLocation;
        }

        public void UndoExecute()
        {
            throw new NotImplementedException();
        }
    }
}
