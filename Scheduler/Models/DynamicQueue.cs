using Scheduler.Services;

namespace Scheduler.Models
{
    internal class DynamicQueue : AbstractQueue
    {
        private readonly Random _random = new Random();
        protected override async Task ExecuteProcess()
        {
            //Выбираем процесс с максимальным приоритетом и временем
            var process = Queue.First();

            if (process.State.GetType() == typeof(ReadyState))
            {
                //Serilog.Log.Information($"Выделяем квант времени для процесса: {process}");
                process.State = new JobState();
                await Task.Delay(ProcessScheduler.QuantTime);
                process.Cpu -= ProcessScheduler.QuantTime;
                if (process.Cpu <= 0)
                {
                    process.Cpu = 0;
                    process.State = new CompletedState();
                }
                else process.State = new ReadyState();
                //Serilog.Log.Information($"После окончания выделенного кванта: {process}\nПеремещаем процесс в конец очереди.");
                Queue.RemoveAt(0);
                Queue.Insert(Queue.Count, process);
            }else if(process.State.GetType() == typeof(WaitState))
            {
                //Serilog.Log.Information($"Процесс: {process} находится в состоянии ожидания.\nИзменяем приоритет и добавляем в конец очереди.");
                if (Queue[0].Priority < 20)
                    Queue[0].Priority += 1;

                //Serilog.Log.Information($"Сортируем процессы в очереди с динамическими приоритетами.");
                Queue = Queue.OrderByDescending(x=>x.Priority).ToList();

            }
        }
    }
}
