using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class FormatDiskCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private Session _session;

        public bool CanExecute(string command) => Regex.IsMatch(command, "format.disk");
        public FormatDiskCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }
        public async Task Execute(string command)
        {
            //TODO Процесс занят. Пофиксить работу
            _session.RequestForAccess();
            await _fileSystem.Formatting();
            _session.User = null;
            _session.CurrentDirectory = Session.RootDirectory;
            _session.IsRoot = false;
        }
        public void Info()
        {
            Console.WriteLine("format.disk\t ~");
        }
    }
}
