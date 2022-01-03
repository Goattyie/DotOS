namespace Scheduler.Services.Commands
{
    internal interface ISystemCommand
    {
        public bool CanExecute(string command);
        public Task Execute(string command);
        public void Info();
    }
}
