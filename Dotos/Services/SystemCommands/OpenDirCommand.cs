using Dotos.Models;
using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class OpenDirCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public OpenDirCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }

        public bool CanExecute(string command) => (Regex.IsMatch(command, "open.dir ([/A-Za-zА-Яа-я1-9])+") || Regex.IsMatch(command, "open.dir .."));

        public async Task Execute(string command)
        {
            //TODO: Добавить проверку на пользователя и каталог
            var path = command.Split(' ')[1];
            if(path == "..")
            {
                var index = _session.CurrentDirectory.GetLastCharIndex('/');
                _session.CurrentDirectory = index != -1 ? _session.CurrentDirectory.Substring(0, index) : "";
            }
            else if (path.Contains('/'))
            {
                await _fileSystem.GetDir(path);
                _session.CurrentDirectory = path;
            }
            else
            {
                await _fileSystem.GetDir(_session.CurrentDirectory + "/" + path);
                _session.CurrentDirectory = _session.CurrentDirectory + "/" + path;
            }
        }
        public void Info()
        {
            Console.WriteLine("open.dir *dirname*\t ~");
        }
    }
}
