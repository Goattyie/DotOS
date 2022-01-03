using Scheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Services
{
    internal class ProcessScheduler
    {
        public static int Iteration = 0;

        public const int QuantTime = 100;
        public ProcessScheduler()
        {
            Queue1 = new();
            Queue2 = new();
        }
        //Очередь с абсолютными приоритетами
        public AbsoluteQueue Queue1 { get; }
        //Очередь с динамическими приоритетами
        public DynamicQueue Queue2 { get; }

        public void AddProcess(List<Process> processes)
        {
            var absulutes = processes.Where(x => x.Priority < 10).ToList();
            var dynamic = processes.Where(x => x.Priority >= 10).ToList();
            Queue1.AddProcess(absulutes);
            Queue2.AddProcess(dynamic);
        }
        public void RemoveProcess(int pid)
        {
            if(Queue1.Queue.Where(x=>x.Pid == pid).Count() == 1)
                Queue1.RemoveProcess(pid);
            else Queue2.RemoveProcess(pid);
        }

        public async void Handle()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (Queue1.Count >= 1)
                    {
                        PrintQueueToLogger();
                        await Queue1.Handle();
                    }
                    else if (Queue2.Count >= 1)
                    {
                        PrintQueueToLogger();
                        await Queue2.Handle();
                    }
                    await Queue1.RestatusProcesses();
                    await Queue2.RestatusProcesses();
                }
            });
        }

        private void PrintQueueToLogger()
        {
            Serilog.Log.Information("Очередь 1:");
            foreach (var process in Queue1.Queue.ToList())
            {
                Serilog.Log.Information(process?.ToString());
            };
            Serilog.Log.Information("Очередь 2:");
            foreach (var process in Queue2.Queue.ToList())
            {
                Serilog.Log.Information(process?.ToString());
            }
        }
    }
}
