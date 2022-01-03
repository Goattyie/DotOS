using Scheduler.Services;

namespace Scheduler.Models
{
    internal class AbsoluteQueue : AbstractQueue
    {
        protected override async Task ExecuteProcess()
        {
            //Выбираем процесс с максимальным приоритетом и временем
            var process = Queue.First();
            //Serilog.Log.Information($"Выделяем квант времени для процесса: {process}");
            var index = Queue.IndexOf(process);
            process.State = new JobState();
            await Task.Delay(ProcessScheduler.QuantTime);
            process.Cpu -= ProcessScheduler.QuantTime;
            if (process.Cpu <= 0)
            {
                process.Cpu = 0;
                process.State = new CompletedState();
            }else process.State = new ReadyState();
            //Serilog.Log.Information($"После окончания выделенного кванта: {process}\nПеремещаем процесс в конец очереди.");
            Queue.RemoveAt(0);
            Queue.Insert(Queue.Count, process);
        }

        public async override Task RestatusProcesses()
        {
            await base.RestatusProcesses();
            if (Count > 0)
                Queue[0].State = new ReadyState();
        }
    }
}
