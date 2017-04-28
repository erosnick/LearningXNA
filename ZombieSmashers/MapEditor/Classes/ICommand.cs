using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.Classes
{
    public interface ICommand
    {
        void Execute(MapSegment segment, Vector2 newPosition);
        void UndoExecute();
    }
}
