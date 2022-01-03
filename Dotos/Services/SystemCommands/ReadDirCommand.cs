using Dotos.Models;
using Dotos.Utils;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dotos.Services.SystemCommands
{
    internal class ReadDirCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public ReadDirCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }

        public bool CanExecute(string command) => Regex.IsMatch(command, "read.dir( [/A-Za-z1-9]+)?");

        public async Task Execute(string command)
        {
            var arr = command.Split(' ');
            FileModel dir;
            if (arr.Length == 1)
            {
                dir = await _fileSystem.GetDir(_session.CurrentDirectory);
            }
            else
            {
                dir = await _fileSystem.GetDir(arr[1]);
            }
            
            var usersData = await _fileSystem.ReadData(_fileSystem.StartUsersByte, _fileSystem.UsersSize);
            var userList = usersData.AsUserList();
            //Если корневая директория
            if (dir.Name == Session.RootDirectory)
            {
                var data = await _fileSystem.ReadData(_fileSystem.StartRootDirectoryByte, _fileSystem.RootDirectorySize);
                data.AsFileList().ForEach(x => Console.WriteLine(x.ToString(userList)));
            }
            else
            {
                var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + dir.Cluster * _fileSystem.ClusterSize, _fileSystem.ClusterSize);
                var list = data.AsFileList();
                list.ForEach(x => Console.WriteLine(x.ToString(userList)));
            }
        }
        public void Info()
        {
            Console.WriteLine("read.dir *dirname*\t ~");
        }
    }
}
