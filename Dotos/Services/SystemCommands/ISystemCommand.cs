using System.Threading.Tasks;

namespace Dotos.Services.SystemCommands
{
    internal interface ISystemCommand
    {
        public bool CanExecute(string command);
        public Task Execute(string command);
        public void Info();
    }
}
