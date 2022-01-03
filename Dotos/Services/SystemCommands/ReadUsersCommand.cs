using Dotos.Models;
using Dotos.Utils;

namespace Dotos.Services.SystemCommands
{
    internal class ReadUsersCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;

        public ReadUsersCommand(FileSystem fs)
        {
            _fileSystem = fs;
        }

        public bool CanExecute(string command) => command == "get.users";

        public async Task Execute(string command)
        {
            var data = await _fileSystem.ReadData(_fileSystem.StartUsersByte, _fileSystem.UsersSize);
            var list = data.AsUserList();
            list.ForEach(x => Console.WriteLine(x));
        }

        public void Info()
        {
            Console.WriteLine("get.users\t get all users info");
        }
    }
}
