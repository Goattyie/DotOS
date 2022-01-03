using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Scheduler.Services.Commands
{
    internal class PrintProcessesCommand : ISystemCommand
    {
        private readonly ProcessScheduler _scheduler;

        public PrintProcessesCommand(ProcessScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "ps");

        public async Task Execute(string command)
        {
            int i = 10;
            while (i > 0)
            {
                Console.Clear();
                Console.WriteLine("Очередь 1:");
                foreach (var process in _scheduler.Queue1.Queue.ToList())
                {
                    Console.WriteLine(process?.ToString());
                };
                Console.WriteLine("Очередь 2:");
                foreach (var process in _scheduler.Queue2.Queue.ToList())
                {
                    Console.WriteLine(process?.ToString());
                }
                await Task.Delay(ProcessScheduler.QuantTime);
                i--;
                if (i == 0)
                { 
                    if (Console.ReadKey(true).Key == ConsoleKey.Z) 
                        i = 10; 
                }

            }
        }

        public void Info()
        {
            Console.WriteLine("pwd");
        }
    }
}
