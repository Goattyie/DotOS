using System.Text.RegularExpressions;

namespace Scheduler.Services.Commands
{
    internal class RemoveProcessCommand : ISystemCommand
    {
        private readonly ProcessScheduler _scheduler;

        public RemoveProcessCommand(ProcessScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "kill [0-9]+");

        public Task Execute(string command)
        {
            var arr = command.Split(' ');
            try { _scheduler.RemoveProcess(int.Parse(arr[1])); } catch { }

            return Task.CompletedTask;
        }

        public void Info()
        {
            Console.WriteLine("remove.process");
        }
    }
}

