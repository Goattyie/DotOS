using Scheduler.Services;

namespace Scheduler.Models
{
    internal abstract class AbstractQueue
    {
        private Random _random = new();
        public List<Process> Queue { get; set; } = new();
        public int Count => Queue.Count;
        public async Task Handle()
        {
            ProcessScheduler.Iteration += 1;
            Serilog.Log.Information($"Итерация: {ProcessScheduler.Iteration}");
            //Очищаем завершенные процессы
            await ExecuteProcess();
            //Serilog.Log.Information($"Удаляем все завершенные процессы, если такие есть.");
            Queue.RemoveAll(x => x.State.GetType() == typeof(CompletedState));
            
        }
        public void AddProcess(List<Process> process)
        {
            Queue.AddRange(process);
        }
        public void RemoveProcess(int pid)
        {
            var val = Queue.First(x => x.Pid == pid);
            if(val.State.GetType() != typeof(JobState))
                Queue.Remove(val);
            else Console.WriteLine("Process working now.");
        }
        protected abstract Task ExecuteProcess();
        public virtual async Task RestatusProcesses()
        {
            for (int i = 0; i < Queue.Count; i++)
            {
                var val = _random.Next(1, 4);
                await Task.Delay(1);

                if (val == 3)
                    Queue[i].State = new WaitState();
                else Queue[i].State = new ReadyState();
            }
        }
    }
}
