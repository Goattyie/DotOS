using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class SignOutCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;

        public bool CanExecute(string command) => Regex.IsMatch(command, "sign.out");

        public SignOutCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }
        public async Task Execute(string command)
        {
            if (_session.User == null)
                throw new Exception("Denied. You not in session.");

            _session.User = null;
            _session.CurrentDirectory = "";
            _session.IsRoot = false;
        }
        public void Info()
        {
            Console.WriteLine("sign.out\t~");
        }
    }
}
