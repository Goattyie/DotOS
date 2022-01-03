namespace Scheduler.Models
{
    internal class Process : IComparable<Process>
    {
        private int _priority;
        public int Pid { get; set; }
        public int Priority { get => _priority; set { if (value > 20) throw new Exception("priority can be 1-20"); _priority = value; } }
        public IState State { get; set; }
        public int Cpu { get;set; }
        public Process(int priority, int time)
        {
            Priority = priority;
            Cpu = time * 100;
            Pid = int.Parse(DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString());
            State = new ReadyState();
        }
        public override string ToString()
        {
            return $"pid: {Pid}\tpriority: {Priority}\tstate: {State.Value}\t cpu: {Cpu}";
        }

        public int CompareTo(Process? other)
        {
            return other?.Priority ?? 0;
        }
    }


    internal interface IState { char Value { get; } }
    internal class ReadyState : IState { char IState.Value => 'r'; }
    internal class JobState : IState { char IState.Value => 'j'; }
    internal class StopState : IState { char IState.Value => 's'; }
    internal class WaitState : IState { char IState.Value => 'w'; }
    internal class CompletedState : IState { char IState.Value => 'c'; }
}
