namespace VRCheat.Commands
{
    public class CommandsBase
    {
        private Command[] _commands;
        public Command[] Commands
        {
            get => _commands;
            set
            {
                if (_commands == null)
                    _commands = value;
            }
        }

        public virtual bool Precondition()
        {
            return true;
        }
    }
}
