using Scheduler.Models;
using System.Text.RegularExpressions;

namespace Scheduler.Services.Commands
{
    internal class CreateProcessCommand : ISystemCommand
    {
        private readonly ProcessScheduler _scheduler;
        private Random _random = new();

        public CreateProcessCommand(ProcessScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "new [1-9]([0-9])* [1-9]([0-9])*");

        public Task Execute(string command)
        {
            var arr = command.Split(' ');
            var count = int.Parse(arr[2]);
            var processList = new List<Process>();
            for (int i = 0; i < count; i++)
            {
                var process = new Process(_random.Next(1, 20), int.Parse(arr[1]));
                Thread.Sleep(1);
                processList.Add(process);
            }
            _scheduler.AddProcess(processList);
            return Task.CompletedTask;
        }

        public void Info()
        {
            Console.WriteLine("new process");
        }
    }
}
