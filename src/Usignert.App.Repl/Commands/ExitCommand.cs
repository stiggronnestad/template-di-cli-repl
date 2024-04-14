using Usignert.CommandLine;

namespace Usignert.App.Repl.Commands
{
    [Command("exit", "Exit the repl.")]
    public sealed class ExitCommand : ICommand
    {
        public void Execute()
        {
            AppRepl.GetCancellationTokenSource().Cancel();
            Environment.Exit(0);
        }
    }
}
