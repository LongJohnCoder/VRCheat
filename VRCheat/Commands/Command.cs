using System.Reflection;
using UnityEngine;

namespace VRCheat.Commands
{
    public class Command
    {
        public readonly CommandsBase Base;
        public readonly KeyCode KeyCode;
        public readonly bool Hold;

        private readonly MethodInfo _method;

        public Command(CommandsBase commandsBase, MethodInfo method, KeyCode keyCode, bool hold = false)
        {
            Base = commandsBase;
            KeyCode = keyCode;
            Hold = hold;

            _method = method;
        }

        public void Execute()
        {
            _method.Invoke(Base, null);
        }
    }
}
