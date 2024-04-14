using Usignert.CommandLine;

namespace Usignert.App.Commands
{
    public abstract class CommandBase : ICommand
    {
        public virtual void Execute() { }
    }
}
