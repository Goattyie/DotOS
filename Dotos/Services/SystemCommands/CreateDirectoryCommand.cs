using Dotos.Models;
using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class CreateDirectoryCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public CreateDirectoryCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }

        public bool CanExecute(string command) => Regex.IsMatch(command, "create.dir ([/A-Za-zА-Яа-я1-9])+");

        public async Task Execute(string command)
        {
            var arr = command.Split(' ');
            var path = arr[1].GetPath();
            var dir = await _fileSystem.GetDir(path.DirectoryPath ?? _session.CurrentDirectory);
            //Если корневая директория
            if (dir.Name == Session.RootDirectory)
                throw new Exception("Can't create directory in this path");
            _session.CanWrite(dir);
            //Проверяем, если ли уже каталог с таким именем;
            var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + dir.Cluster * _fileSystem.ClusterSize, _fileSystem.ClusterSize);
            var list = data.AsFileList();
            list.ForEach(x=> { if (x.Name == path.Filename && x.Type == 1) throw new Exception("Directory already exist."); });
            //Находим свободный кластер для записи нового файла в область данных
            var cluster = await _fileSystem.GetFreeCluster();
            var newDir = new FileModel() { Name = path.Filename, Type = 1, Attributes = 0b_111000, DateTime = DateTime.Now.ToLong(), UserId = _session.User.Id, Cluster = cluster, Size = 0 };
            //Записываем, что кластер является концом файла, а так же в эту область папку
            await _fileSystem.WriteData(1.ToBytes(), _fileSystem.StartTableByte + cluster * 4 - 4);
            await _fileSystem.WriteData(1.ToBytes(), _fileSystem.StartTableByte + _fileSystem.TableSize + cluster * 4 - 4);
            await _fileSystem.WriteData((byte[])newDir, dir.Cluster * _fileSystem.ClusterSize + _fileSystem.StartDataAreaByte, dir.Cluster * _fileSystem.ClusterSize + _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize);

            await _fileSystem.ResizeParentDirectory(dir, dir.Size + 33);
        }

        public void Info()
        {
            Console.WriteLine("create.dir *name*\t create directory");
        }
    }
}
