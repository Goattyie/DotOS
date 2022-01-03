using Dotos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dotos.Services.SystemCommands
{
    internal class ReadFileCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public ReadFileCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }

        public bool CanExecute(string command) => Regex.IsMatch(command, "read.file [/A-Za-zА-Яа-я1-9]+");
        public async Task Execute(string command)
        {
            var path = command.Split(' ')[1].GetPath();

            //Получаем файлы текущей директории
            var directory = await _fileSystem.GetDir(path.DirectoryPath ?? _session.CurrentDirectory);
            var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * directory.Cluster, _fileSystem.ClusterSize);
            var list = data.AsFileList();
            var file = list.Find(x => x.Name == path.Filename && x.Type == 0) ?? throw new Exception($"File {command.Split(' ')[1]} not found");
            _session.CanRead(file);
            var cluster = file.Cluster;
            //считываем данные кластера
            while (true)
            {
                var startPosition = _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * cluster;
                var fileData = await _fileSystem.ReadData(startPosition, _fileSystem.ClusterSize);
                Console.WriteLine(fileData.ToStr());

                //Смотрим на статус кластера
                var clustData = await _fileSystem.ReadData(_fileSystem.StartTableByte + cluster * 4 - 4, 4);
                cluster = clustData.ToInt();

                if (cluster == 1)
                    break;
            }
        }
        public void Info()
        {
            Console.WriteLine("read.file *filename*\t ~");
        }
    }
}
