using Usignert.CommandLine;

namespace Usignert.App.Repl.Commands
{
    [Command("clear", "Clear the terminal.")]
    public sealed class ClearCommand : ICommand
    {
        public void Execute()
        {
            Console.Clear();
        }
    }
}
