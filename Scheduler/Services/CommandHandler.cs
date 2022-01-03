using Scheduler.Services.Commands;

namespace Scheduler.Services
{
    internal class CommandHandler
    {
        private readonly IEnumerable<ISystemCommand> _systemCommands;

        public CommandHandler(IEnumerable<ISystemCommand> systemCommands)
        {
            _systemCommands = systemCommands;
        }

        public async Task Handle(string? command)
        {
            if (command == null) return;
            if (command == "help") { _systemCommands.ToList().ForEach(x => x.Info()); return; }

            var call = _systemCommands.FirstOrDefault(x => x.CanExecute(command));

            if (call != null)
            {
                try
                {
                    await call.Execute(command);
                }catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else Console.WriteLine($"{command}: unknown command");
        }
    }
}
