using Scheduler.Models;
using System.Text.RegularExpressions;

namespace Scheduler.Services.Commands
{
    internal class ChangePriorityCommand : ISystemCommand
    {
        private readonly ProcessScheduler _scheduler;

        public ChangePriorityCommand(ProcessScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "renice [1-9]([0-9])* [1-9]([0-9])*");

        public Task Execute(string command)
        {
            var arr = command.Split(' ');

            var index = _scheduler.Queue1.Queue.IndexOf(_scheduler.Queue1.Queue.First(x => x.Pid == int.Parse(arr[1])));
            if (index != -1)
            {
                _scheduler.Queue1.Queue[index].Priority = int.Parse(arr[2]);
            }
            else
            {
                index = _scheduler.Queue2.Queue.IndexOf(_scheduler.Queue1.Queue.First(x => x.Pid == int.Parse(arr[1])));
                if (index != -1)
                {
                    _scheduler.Queue2.Queue[index].Priority = int.Parse(arr[2]);
                }
                else throw new Exception("Process with this pid not exist.");
            }
            
            return Task.CompletedTask;
        }

        public void Info()
        {
            Console.WriteLine("change.priority");
        }
    }
}
