using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.Classes
{
    public class CommandManager
    {
        private static CommandManager commandManager;
        private static CommandManager Instance
        {
            get
            {
                if (commandManager == null)
                {
                    commandManager = new CommandManager();
                }

                return commandManager;
            }
        }

        static CommandManager()
        {
            undoList = new Stack<ICommand>();
            redoList = new Stack<ICommand>();
        }

        private static Stack<ICommand> undoList;
        private static Stack<ICommand> redoList;

        public static void ExecuteCommand(ICommand command)
        {
            command.Execute();
            undoList.Push(command);
        }

        public static void UndoCommand()
        {
            if (undoList.Count > 0)
            {
                var command = undoList.Pop();
                command.UndoExecute();

                redoList.Push(command);
            }
        }

        public static void RedoCommand()
        {
            if (redoList.Count > 0)
            {
                redoList.Pop().Execute();
            }
        }
    }
}
